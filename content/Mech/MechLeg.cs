namespace TC2.Base.Components;



public static partial class MechLeg
{
    [IComponent.Data(Net.SendType.Reliable)]
    public partial struct Data : IComponent
    {
        
        public Data()
        {

        }
    }



    [ISystem.Update(ISystem.Mode.Single)]
    public static void Update(Entity ent_joint, [Source.Owned] ref Joint.Base joint, [Source.Owned, Original] ref Joint.Gear gear, [Source.Owned] ref Joint.Pivot pivot,
        [Source.Parent] ref MechLeg.Data data, [Source.Parent, Original] ref Joint.Gear cabGear)
    {
        
    }
}