namespace TC2.Base.Components
{
    public static partial class Bertha
	{
        [IComponent.Data(Net.SendType.Reliable)]
        public partial struct Data : IComponent
        {
            public float gearAngle = 0f;
            public float minAngle = -0.8f;
            public float maxAngle = 0.25f;
            public float speedMultiplier = 0.001f;

            public Data()
            {

            }
        }

        [IComponent.Data(Net.SendType.Unreliable)]
        public partial struct State : IComponent
        {
            public bool isLoaded = false;

            public State()
            {

            }
        }

        public struct ConfigureRPC : Net.IRPC<Bertha.State>
        {
            public bool fire;
            public bool load;
            public Inventory1.Data magazine;
            public FixedArray8<Entity> parts;
            public Gun.State gun_state;

#if SERVER
            public void Invoke(ref NetConnection connection, Entity entity, ref Bertha.State data)
            {
                if (magazine.entity.GetParent(Relation.Type.Child, 0).GetParent(Relation.Type.Instance, 0) != entity)
                {
                    App.WriteLine("Magazine entity root not equal to bertha entity");
                    return;
                }

                ref var control = ref entity.GetComponent<Control.Data>();
                ref var storage = ref entity.GetTrait<Storage.Data, Inventory1.Data>();

                if (!storage.IsNull() && !magazine.IsNull() && load)
                {
                    if (magazine.resource.quantity < 1 && storage.resource.quantity < 1)
                    {
                        int bags = 0;

                        for (int i = 0; i < parts.Length; i++)
                        {
                            if (parts[i] != Entity.None)
                            {
                                if (parts[i].GetFullName() == "bertha_powderbag")
                                {
                                    bags += 1;
                                }
                                parts[i].Delete();
                            }
                        }

                        storage.resource.material = Material.GetMaterial("ammo_bertha_shell").id;
                        storage.resource.quantity = 1;

                        data.isLoaded = true;
                        control.keyboard.SetKeyPressed(Keyboard.Key.Reload, true);
                    }
                }
                else if (this.fire && gun_state.stage == Gun.Stage.Ready)
                {
                    control.mouse.SetKeyPressed(Mouse.Key.Left, true);
                    data.isLoaded = false;
                }

                if (!storage.IsNull())
                {
                    storage.Sync<Inventory1.Data, Storage.Data>(entity);
                }
                else
                {
                    App.WriteLine("magazine is null", App.Color.Red);
                }

                data.Sync(entity);
                control.Sync(entity);
            }
#endif
        }

        public struct GunRPC : Net.IRPC<Gun.Data>
        {
            public float reloadTime;
            public float velocity;


#if SERVER
            public void Invoke(ref NetConnection connection, Entity entity, ref Gun.Data data)
            {
                data.reload_interval = reloadTime;
                data.velocity_multiplier = velocity;
                data.Sync(entity);
            }
#endif
        }

        [ISystem.EarlyUpdate(ISystem.Mode.Single), HasTag("turret", true, Source.Modifier.Owned)]
        public static void Update([Source.Owned, Original] ref Joint.Gear gear, [Source.Shared] ref Axle.State axle, [Source.Shared] ref Bertha.Data data)
		{
            float tempRot = data.gearAngle + (axle.angular_velocity * data.speedMultiplier);
            if (tempRot < data.maxAngle && tempRot > data.minAngle)
            {
                data.gearAngle = tempRot;
            }


            gear.rotation = data.gearAngle;
        }

#if CLIENT
        public struct BerthaGUI: IGUICommand
        {
            public Entity ent_bertha;
            public Bertha.State bertha_state;
            public Bertha.Data bertha_data;
            public Inventory1.Data inventory;
            public Gun.State gun_state;
            public Gun.Data gun_data;

            public void Draw()
            {
                using (var window = GUI.Window.Interaction("Bertha", this.ent_bertha))
                {
                    this.StoreCurrentWindowTypeID(order: -100);

                    Span<Entity> children = stackalloc Entity[6];
                    this.ent_bertha.GetChildren(ref children, Relation.Type.Instance);

                    int shells = 0;
                    int bags = 0;
                    Entity ent_gun = Entity.None;

                    Span<Entity> parts = stackalloc Entity[6];

                    for (int i = 0; i < children.Length; i++)
                    {
                        if (!children[i].HasTag("turret"))
                        {
                            var child = children[i].GetChild(Relation.Type.Child);
                            if (child.IsAlive())
                            {
                                if (child.GetFullName() == "bertha_shell")
                                {
                                    shells += 1;
                                    parts[i] = child;
                                }
                                else if (child.GetFullName() == "bertha_powderbag")
                                {
                                    bags += 1;
                                    parts[i] = child;
                                }
                            }
                        }
                        else
                        {
                            ent_gun = children[i].GetChild(Relation.Type.Child);
                            continue;
                        }
                    }

                    using (GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth(), GUI.GetRemainingHeight())))
                    {
                        using (var group_buttons = GUI.Group.New(size: new Vector2(96, 46)))
                        {

                            using (var load_slot = GUI.Group.New(size: new Vector2(96, 46)))
                            {
                                GUI.DrawBackground(GUI.tex_frame, load_slot.GetOuterRect(), new(4));

                                using (var button = GUI.CustomButton.New("Load", load_slot.size))
                                {
                                    GUI.DrawButton("Load", new Vector2(96, 46));

                                    bool doneCycling = Client.GetRegion().GetWorldTime() > gun_state.next_cycle; ;

                                    if (button.pressed)
                                    {
                                        if (shells == 1 && bags > 0 && !bertha_state.isLoaded && doneCycling)
                                        {
                                            var rpc = new Bertha.ConfigureRPC
                                            {
                                                magazine = inventory,
                                                load = true,
                                                parts = FixedArray.ToFixedArray8<Entity>(ref parts),
                                            };
                                            rpc.Send(this.ent_bertha);
                                        }
                                    }

                                    if (button.hovered)
                                    {
                                        if (!doneCycling)
                                        {
                                            using (GUI.Tooltip.New())
                                            {
                                                GUI.TitleCentered("Cycling", color: Color32BGRA.Red);
                                            }
                                        }
                                        else if (!bertha_state.isLoaded)
                                        {
                                            using (GUI.Tooltip.New())
                                            {
                                                string msg = "Missing components";
                                                string title = "Invalid Shell";
                                                Color32BGRA color = Color32BGRA.Grey;
                                                if (shells == 0 && bags == 0)
                                                {

                                                }
                                                else if (shells < 1)
                                                {
                                                    msg = "Missing warhead";
                                                    color = Color32BGRA.Red;
                                                }
                                                else if (shells > 1)
                                                {
                                                    msg = "Can only be one warhead";
                                                    color = Color32BGRA.Red;
                                                }
                                                else if (bags < 1)
                                                {
                                                    msg = "Missing powder bag";
                                                    color = Color32BGRA.Red;
                                                }
                                                else
                                                {
                                                    msg = "Valid shell";
                                                    title = "Valid shell";
                                                    color = Color32BGRA.Green;

                                                    GUI.Title(title, color: color);
                                                    GUI.Text(msg, color: color);
                                                    GUI.Text("Reload time: " + (10f + (5f * bags) + " s"), color: color);

                                                    var rpc = new Bertha.GunRPC
                                                    {
                                                        reloadTime = 10f + (5f * bags),
                                                        velocity = 100 + (100 * bags)
                                                    };
                                                    rpc.Send(ent_gun);

                                                    goto skip;
                                                }
                                                GUI.Title(title, color: color);
                                                GUI.Text(msg, color: color);
                                            skip:;
                                            }
                                        }
                                    }
                                }
                            }
                            
                            using (var fire_slot = GUI.Group.New(size: new Vector2(96, 46)))
                            {
                                bool canShoot = Client.GetRegion().GetWorldTime() > gun_state.next_reload + 0.5f && bertha_state.isLoaded;
                                
                                using (var button = GUI.CustomButton.New("Fire", fire_slot.size))
                                {
                                    GUI.DrawButton("Fire", new Vector2(96, 46));

                                    if (button.pressed && canShoot)
                                    {
                                        var rpc = new Bertha.ConfigureRPC
                                        {
                                            magazine = inventory,
                                            fire = true,
                                            gun_state = gun_state,
                                        };
                                        rpc.Send(this.ent_bertha);
                                    }

                                    if (button.hovered)
                                    {
                                        if (!canShoot)
                                        {
                                            using (GUI.Tooltip.New())
                                            {
                                                GUI.Title("Not loaded", color: Color32BGRA.Red);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        GUI.SameLine();
                        using (var infoDisplay = GUI.Group.New(size: new Vector2(400, 96 )))
                        {
                            GUI.DrawBackground(GUI.tex_panel, infoDisplay.GetOuterRect(), new(4));

                            using (var angleGauge = GUI.Group.New(size: new Vector2(96, 96)))
                            {
                                GUI.DrawSprite("bertha_gauge100", angleGauge.GetInnerRect());

                                float deg = bertha_data.gearAngle;
                                float len = 50;

                                Vector2 offset = new Vector2(angleGauge.GetInnerRect().a.X, window.group.GetInnerRect().a.Y);

                                GUI.DrawLine2(offset + new Vector2(21, 61), offset + new Vector2(21 + ((float)Math.Cos(deg) * len), 61 + ((float)Math.Sin(deg)) * len), Color32BGRA.Red, Color32BGRA.Red, 5, 0, true);

                            }

                            GUI.SameLine();
                            GUI.Text("Elevation: " + (-bertha_data.gearAngle * Maths.rad2deg).ToString("0.00"));
                            GUI.ResetLine(95,10);

                            using (var shellInfo = GUI.Group.New(size: new Vector2(300,85), padding: new Vector2(6,3)))
                            {
                                if (!bertha_state.isLoaded)
                                {
                                    GUI.DrawBackground(GUI.tex_frame, shellInfo.GetOuterRect(), new(4));
                                    GUI.TitleCentered("No shell loaded", color: Color32BGRA.Red);
                                }
                                else
                                {
                                    float timeToLoad = -((gun_state.next_reload - gun_data.reload_interval) - Client.GetRegion().GetWorldTime()) / gun_data.reload_interval;

                                    Color32BGRA col = Color32BGRA.Lerp(Color32BGRA.Red, Color32BGRA.Green, timeToLoad);

                                    if (timeToLoad < 1)
                                    {
                                        GUI.DrawBackground(GUI.tex_frame, shellInfo.GetOuterRect(), new(4));
                                        GUI.TitleCentered("Loading " + timeToLoad.ToString("##.0%") , color: col);
                                        GUI.TextCentered((gun_state.next_reload - Client.GetRegion().GetWorldTime()).ToString("00.0") + " s", new Vector2(0.5f, 0.75f), color: col);
                                    }
                                    else
                                    {
                                        GUI.DrawBackground(GUI.tex_frame, shellInfo.GetOuterRect(), new(4));
                                        GUI.Title("Chamber Info");
                                        GUI.Text("Warhead: ");
                                        GUI.SameLine(); GUI.DrawSprite("bertha_shell");
                                        GUI.SameLine(); GUI.Text(" Standard warhead");
                                        GUI.Text("Velocity: " + gun_data.velocity_multiplier + " m/s");

                                        
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [ISystem.EarlyGUI(ISystem.Mode.Single)]
        public static void OnGUI(ISystem.Info info, Entity ent_interactable, [Source.Parent] in Interactable.Data interactable,
            [Source.Parent] ref Bertha.State state, [Source.Parent] ref Bertha.Data data,
            [Source.Owned] ref Gun.Data gun, [Source.Owned] ref Gun.State gun_state, [Source.Owned, Pair.Of<Gun.Data>] ref Inventory1.Data inventory_magazine)
        {
            if (interactable.show)
            {
                var gui = new BerthaGUI()
                {
                    ent_bertha = ent_interactable,
                    bertha_state = state,
                    inventory = inventory_magazine,
                    bertha_data = data,
                    gun_data = gun,
                    gun_state = gun_state
                };
                gui.Submit();
            }
        }
#endif
    }
}