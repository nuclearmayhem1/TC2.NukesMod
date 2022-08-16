using Keg.Engine.Game;

namespace TC2.Base.Components;

public static partial class Jetpack
{

    [IComponent.Data(Net.SendType.Reliable)]
    public partial struct Data : IComponent
    {
        public float thrust = 150;
        public float thrust_multiplier = 1000;
        [Statistics.Info("Fuel capacity", "How much fuel fits into the tank at standard compression", comparison: Statistics.Comparison.Higher)]
        public float fuel_base = 10;
        [Statistics.Info("Fuel compression", "How much space 1 unit of fuel takes up in the tank", comparison: Statistics.Comparison.Higher)]
        public float fuel_multiplier = 1;
        public float max_fuel { get { return fuel_base * fuel_multiplier; } set { } }
        [Statistics.Info("Cooldown", "How long until next pulse", comparison: Statistics.Comparison.Lower)]
        public float cooldown = 0;
        [Statistics.Info("Refuel cooldown", "How long you need to be grounded in order to refuel the jetpack", comparison: Statistics.Comparison.Lower)]
        public float refuel_cooldown = 10;
        [Statistics.Info("Refuel speed", "How may units of fuel you refuel per second", format: "{0:0.##}s", comparison: Statistics.Comparison.Higher)]
        public float refuel_speed = 1;
        [Statistics.Info("Fuel efficiency", "How much fuel is used for standard thrust", comparison: Statistics.Comparison.Lower)]
        public float efficiency = 1f;
        [Statistics.Info("Fuel usage", "How much fuel is used, thrust is directly related to fuel usage")]
        public float fuel_used = 1;
        [Statistics.Info("Control fuel usage", "How much fuel is used to manouver")]
        public float control_fuel_used = 0.1f;
        [Statistics.Info("Control surface", "Control surfaces change the direction of your velocity for free", comparison: Statistics.Comparison.Higher)]
        public float control_surface = 0;
        [Statistics.Info("Tank type", "What type of fuel tank is being used")]
        public TankType tank_type = TankType.Standard;
        [Statistics.Info("Operation mode", "Determines what type of flight the jetpack uses")]
        public Operation operation = Operation.Continious;
        public float velocityPenalty = 1.1f;
        public float air_time_threshold = 0.1f;

        public Data()
        {

        }
    }

    [IComponent.Data(Net.SendType.Reliable)]
    public partial struct State : IComponent
    {
        public float fuel = 0;
        public float next_cycle = 0;
        public float next_refuel = 0;

        public float multiplier = 0;

        public State()
        {

        }
    }

    public enum TankType : byte
    {
        Undefined = 0,

        Standard,
        Cryogenic,
        Compressed,
        Lightweight,

    }

    public enum Operation : byte
    {
        Undefined = 0,

        Pulse,
        Continious,
        Hover,

    }

    [ISystem.Update(ISystem.Mode.Single)]
    public static void WhenEquiped(Entity entity, ISystem.Info info, [Source.Parent<Equip.Data>] in Equipment.Data equip, [Source.Owned] ref Control.Data control, [Source.Owned] ref Body.Data body,
        [Source.Owned] ref Transform.Data transform, [Source.Owned] ref Torso.Data torso, [Source.Parent<Equip.Data>] ref Jetpack.Data data, [Source.Parent<Equip.Data>] ref Jetpack.State state
        , [Source.Owned, Pair.Of<Storage.Data>] ref Inventory8.Data inventory)
    {



        if (torso.air_time > data.air_time_threshold)
        {
            state.next_refuel = info.WorldTime + data.refuel_cooldown;
        }
        else if (state.next_refuel < info.WorldTime && state.fuel < data.max_fuel)
        {
            for (int i = 0; i < inventory.resources.Length; i++)
            {
                if (inventory.resources[i].material == new Material.Handle("phlogiston"))
                {
                    float ammount = Math.Min(data.refuel_speed * info.DeltaTime, inventory.resources[i].quantity);
                    state.fuel += ammount;
                    inventory.resources[i].quantity -= ammount;
                    break;
                }
            }
        }


        switch (data.operation)
        {
            case Operation.Undefined:
                break;
            case Operation.Pulse:
                break;
            case Operation.Continious:



                Vector2 force = Vector2.Zero;
                float fuelUsed = 0;
                Vector2 velocity = body.GetVelocity();



                float multY = (1 / MathF.Pow(data.velocityPenalty, -velocity.Y));
                state.multiplier = multY;
                float thrustY = (data.thrust * data.thrust_multiplier);
                float thrustX = (data.thrust * data.thrust_multiplier);

                if (control.keyboard.GetKey(Keyboard.Key.MoveUp))
                {
                    force.Y -= thrustY * data.fuel_used * multY;
                    fuelUsed += data.fuel_used * data.efficiency;
                }
                if (control.keyboard.GetKey(Keyboard.Key.MoveDown))
                {
                    force.Y += thrustY * data.control_fuel_used;
                    fuelUsed += data.fuel_used * data.efficiency;
                }
                if (control.keyboard.GetKey(Keyboard.Key.MoveRight))
                {
                    force.X += thrustX * data.control_fuel_used;
                    fuelUsed += data.control_fuel_used * data.efficiency;
                }
                if (control.keyboard.GetKey(Keyboard.Key.MoveLeft))
                {
                    force.X -= thrustX * data.control_fuel_used;
                    fuelUsed += data.control_fuel_used * data.efficiency;
                }

                fuelUsed *= info.DeltaTime;
                force *= info.DeltaTime;

                if (fuelUsed > state.fuel)
                {
                    force *= state.fuel / fuelUsed;
                    state.fuel = 0;
                    body.AddForce(force);
                    break;
                }
                state.fuel -= fuelUsed;
                body.AddForce(force);
                break;
            case Operation.Hover:
                break;
            default:
                break;
        }


    }


}