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
            public Gun.State gun_state;
            public Inventory1.Data inventory;
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
                                            bertha_state.isLoaded = true;
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
                                        gun_state.stage = Gun.Stage.Fired;
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

        [ISystem.EarlyGUI(ISystem.Mode.Single), HasTag("barrel", true, Source.Modifier.Owned)]
        public static void OnGUI(ISystem.Info info, Entity entity,
            [Source.Shared] ref Bertha.State state,
            [Source.Owned] ref Gun.Data gun, [Source.Owned, Pair.Of<Gun.Data>] ref Inventory1.Data inventory_magazine, [Source.Owned] ref Gun.State gun_state)
        {
            if (interactable.show)
            {
                var gui = new BerthaGUI()
                {
                    ent_bertha = entity,
                    bertha_state = state,
                    gun_state = gun_state,
                    inventory = inventory_magazine
                };
                gui.Submit();
            }
        }
#endif
    }
}