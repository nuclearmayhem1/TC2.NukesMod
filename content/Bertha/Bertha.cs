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
                //ref var magazine = ref entity.GetChild(Relation.Type.Instance).GetChild(Relation.Type.Child).GetTrait<Gun.Data, Inventory1.Data>();

                control.mouse.SetKeyPressed(Mouse.Key.Left, true);

                if (this.load && !storage.IsNull() && !magazine.IsNull())
                {
                    if (magazine.resource.quantity < 1 && storage.resource.quantity < 1)
                    {
                        storage.resource.material = Material.GetMaterial("ammo_bertha_shell").id;
                        storage.resource.quantity = 1;

                        data.isLoaded = true;
                        control.keyboard.SetKeyPressed(Keyboard.Key.Reload, true);

                        for (int i = 0; i < parts.Length; i++)
                        {
                            if (parts[i] != Entity.None)
                            {
                                parts[i].Delete();
                            }
                        }

                    }
                }
                else if (this.fire)
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

            public void Draw()
            {
                using (var window = GUI.Window.Interaction("Bertha", this.ent_bertha))
                {
                    this.StoreCurrentWindowTypeID(order: -100);



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

                                    if (button.pressed)
                                    {
                                        if (!bertha_state.isLoaded)
                                        {
                                            Span<Entity> children = stackalloc Entity[6];
                                            this.ent_bertha.GetChildren(ref children, Relation.Type.Instance);

                                            int shells = 0;
                                            int bags = 0;

                                            Span<Entity> parts = stackalloc Entity[6];

                                            for (int i = 0; i < children.Length; i++)
                                            {
                                                if (!children[i].HasTag("turret"))
                                                {
                                                    var child = children[i].GetChild(Relation.Type.Child);
                                                    if (child.IsAlive())
                                                    {
                                                        App.WriteLine(child.GetFullName());
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
                                                    continue;
                                                }
                                            }

                                            if (shells == 1 && bags > 0)
                                            {
                                                var rpc = new Bertha.ConfigureRPC
                                                {
                                                    magazine = inventory,
                                                    load = true,
                                                    parts = FixedArray.ToFixedArray8<Entity>(ref parts)
                                                };
                                                rpc.Send(this.ent_bertha);


                                            }
                                        }
                                    }

                                    if (button.hovered)
                                    {
                                        if (!bertha_state.isLoaded)
                                        {
                                            Span<Entity> children = stackalloc Entity[6];
                                            this.ent_bertha.GetChildren(ref children, Relation.Type.Instance);

                                            int shells = 0;
                                            int bags = 0;

                                            Span<Entity> parts = stackalloc Entity[6];

                                            for (int i = 0; i < children.Length; i++)
                                            {
                                                if (!children[i].HasTag("turret"))
                                                {
                                                    var child = children[i].GetChild(Relation.Type.Child);
                                                    if (child.IsAlive())
                                                    {
                                                        App.WriteLine(child.GetFullName());
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
                                                    continue;
                                                }
                                            }

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
                                                }
                                                GUI.Title(title, color: color);
                                                GUI.Text(msg, color: color);
                                            }

                                        }
                                    }
                                }
                            }

                            using (var fire_slot = GUI.Group.New(size: new Vector2(96, 46)))
                            {
                                GUI.DrawBackground(GUI.tex_frame, fire_slot.GetOuterRect(), new(4));

                                using (var button = GUI.CustomButton.New("Fire", fire_slot.size))
                                {
                                    GUI.DrawButton("Fire", new Vector2(96, 46));

                                    if (button.pressed)
                                    {
                                        var rpc = new Bertha.ConfigureRPC
                                        {
                                            magazine = inventory,
                                            fire = true
                                        };
                                        rpc.Send(this.ent_bertha);

                                    }

                                    if (button.hovered)
                                    {

                                    }
                                }
                            }
                        }
                        GUI.SameLine();
                        using (var infoDisplay = GUI.Group.New(size: new Vector2(192, 96 )))
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
                            GUI.NewLine();
                            using (var shellInfo = GUI.Group.New(size: new Vector2(16,16)))
                            {
                                GUI.DrawBackground(GUI.tex_frame, shellInfo.GetInnerRect(), new(4));
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
                    bertha_data = data
                };
                gui.Submit();
            }
        }
#endif
    }
}