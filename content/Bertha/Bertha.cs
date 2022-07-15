namespace TC2.Base.Components
{
    public static partial class Bertha
	{
        [IComponent.Data(Net.SendType.Reliable)]
        public partial struct Data : IComponent
        {
            public float gearAngle = 0f;
            public float minAngle = -0.75f;
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
#if SERVER
            public void Invoke(ref NetConnection connection, Entity entity, ref Bertha.State data)
            {

                ref var control = ref entity.GetComponent<Control.Data>();
                control.mouse.SetKeyPressed(Mouse.Key.Left, true);

                if (this.load)
                {
                    if (!data.isLoaded)
                    {
                        data.isLoaded = true;
                        control.keyboard.SetKeyPressed(Keyboard.Key.Reload, true);
                    }
                    this.load = false;
                }
                else if (this.fire)
                {
                    control.mouse.SetKeyPressed(Mouse.Key.Left, true);
                    fire = false;
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
                                        App.WriteLine("test1");
                                        if (!bertha_state.isLoaded)
                                        {

                                            var rpc = new Bertha.ConfigureRPC
                                            {
                                                load = true
                                            };
                                            rpc.Send(this.ent_bertha);

                                            App.WriteLine("loaded bertha");
                                        }
                                    }

                                    if (button.hovered)
                                    {

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
                                        App.WriteLine("test2");

                                        var rpc = new Bertha.ConfigureRPC
                                        {
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
                        GUI.NewLine();
                    }

                }
            }
        }

        [ISystem.EarlyGUI(ISystem.Mode.Single)]
        public static void OnGUI(ISystem.Info info, Entity ent_interactable, [Source.Parent] in Interactable.Data interactable,
            [Source.Parent] ref Bertha.State state,
            [Source.Owned] ref Gun.Data gun, [Source.Owned] ref Gun.State gun_state, [Source.Owned, Pair.Of<Gun.Data>] ref Inventory1.Data inventory_magazine)
        {
            if (interactable.show)
            {
                var gui = new BerthaGUI()
                {
                    ent_bertha = ent_interactable,
                    bertha_state = state,
                    inventory = inventory_magazine
                };
                gui.Submit();
            }
        }
#endif
    }
}