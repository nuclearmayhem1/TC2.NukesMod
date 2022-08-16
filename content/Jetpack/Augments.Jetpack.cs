using TC2.Base.Components;

namespace TC2.NukesMod;

public sealed partial class ModInstance
{
    private static void RegisterJetpackAugments(ref List<Augment.Definition> definitions)
    {
        definitions.Add(Augment.Definition.New<Jetpack.Data>
        (
            identifier: "jetpack.expaded_fueltank",
            category: "Jetpack (fuel)",
            name: "Expanded fuel tank",
            description: "Increases fuel capacity.",

            validate: static (ref Augment.Context context, in Jetpack.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
            {
                ref var amount = ref handle.GetData<float>();
                amount = Maths.Clamp(amount, 1.00f, 5.00f);

                return true;
            },

#if CLIENT
            draw_editor: static (ref Augment.Context context, in Jetpack.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
            {
                ref var amount = ref handle.GetData<float>();
                return GUI.SliderFloat("Value", ref amount, 1.00f, 5.00f);
            },
#endif
            
            can_add: static (ref Augment.Context context, in Jetpack.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
            {
                return augments.GetCount(handle) < 3;
            },

            finalize: static (ref Augment.Context context, ref Jetpack.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
            {
                ref var amount = ref handle.GetData<float>();
                data.fuel_base += amount;

                ref var body = ref context.GetComponent<Body.Data>();
                if (!body.IsNull())
                {
                    body.mass_extra += amount * 1f;
                }

                return true;
            },

            apply_0: static (ref Augment.Context context, ref Jetpack.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
            {
                ref var amount = ref handle.GetData<float>();
                foreach (ref readonly var requirement in context.requirements_old)
                {
                    switch (requirement.type)
                    {
                        case Crafting.Requirement.Type.Resource:
                            {
                                ref var material = ref requirement.material.GetDefinition();
                                if (!material.flags.HasAll(Material.Flags.Manufactured) && material.type == Material.Type.Metal)
                                {
                                    context.requirements_new.Add(Crafting.Requirement.Resource(requirement.material, 0.5f * amount));
                                }
                            }
                            break;

                        case Crafting.Requirement.Type.Work:
                            {
                                switch (requirement.work)
                                {
                                    case Work.Type.Smithing:
                                        {
                                            context.requirements_new.Add(Crafting.Requirement.Work(requirement.work, 50.00f * amount, (byte)MathF.Ceiling(5.00f * amount)));
                                        }
                                        break;
                                    case Work.Type.Assembling:
                                        {
                                            context.requirements_new.Add(Crafting.Requirement.Work(requirement.work, 25.00f * amount, (byte)MathF.Ceiling(5.00f * amount)));
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                }
            }
        ));

        definitions.Add(Augment.Definition.New<Jetpack.Data>
        (
            identifier: "jetpack.reinforced_fueltank",
            category: "Jetpack (fuel)",
            name: "Reinforced fuel tank",
            description: "Reinforces fuel tank to allow higher compression and increases weight, due to higher pressures refuling speed is slightly reduced",

            validate: static (ref Augment.Context context, in Jetpack.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
            {
                ref var amount = ref handle.GetData<float>();
                amount = Maths.Clamp(amount, 0, 1);

                return true;
            },

#if CLIENT
            draw_editor: static (ref Augment.Context context, in Jetpack.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
            {
                ref var amount = ref handle.GetData<float>();
                return GUI.SliderFloat("Value", ref amount, 0, 1f);
            },
#endif

            can_add: static (ref Augment.Context context, in Jetpack.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
            {
                return data.tank_type == Jetpack.TankType.Standard;
            },

            finalize: static (ref Augment.Context context, ref Jetpack.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
            {
                ref var amount = ref handle.GetData<float>();
                data.fuel_multiplier += amount;
                data.refuel_speed *= 1 - (amount * 0.2f);

                ref var body = ref context.GetComponent<Body.Data>();
                if (!body.IsNull())
                {
                    body.mass_multiplier += amount * 0.3f;
                }

                return true;
            },

            apply_0: static (ref Augment.Context context, ref Jetpack.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
            {
                ref var amount = ref handle.GetData<float>();
                ref var material_iron = ref Material.GetMaterial("iron_ingot");



                var ingot_amount = 0.00f;
                foreach (ref var requirement in context.requirements_new)
                {
                    if (requirement.type == Crafting.Requirement.Type.Work)
                    {
                        requirement.amount *= 1.50f;
                        requirement.difficulty += 5.00f;
                    }
                    else if (requirement.type == Crafting.Requirement.Type.Resource)
                    {
                        if (requirement.material.id == material_iron.id)
                        {
                            ingot_amount += requirement.amount;
                            requirement = default;
                        }
                    }
                }
                data.tank_type = Jetpack.TankType.Compressed;
                var total_amount = 3.00f + (ingot_amount * (1 + amount) * 0.30f);
                context.requirements_new.Add(Crafting.Requirement.Resource("smirgl_ingot", total_amount));
            }
        ));

        definitions.Add(Augment.Definition.New<Jetpack.Data>
        (
            identifier: "jetpack.lightweight_fueltank",
            category: "Jetpack (fuel)",
            name: "Lightweight fuel tank",
            description: "Weakens fuel tank which reduces compression and reduces weight, due to lower pressures refuling speed is slightly increased",

            validate: static (ref Augment.Context context, in Jetpack.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
            {
                ref var amount = ref handle.GetData<float>();
                amount = Maths.Clamp(amount, 0, 1);

                return true;
            },

#if CLIENT
            draw_editor: static (ref Augment.Context context, in Jetpack.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
            {
                ref var amount = ref handle.GetData<float>();
                return GUI.SliderFloat("Value", ref amount, 0, 1f);
            },
#endif

            can_add: static (ref Augment.Context context, in Jetpack.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
            {
                return data.tank_type == Jetpack.TankType.Standard;
            },

            finalize: static (ref Augment.Context context, ref Jetpack.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
            {
                ref var amount = ref handle.GetData<float>();
                data.fuel_multiplier -= amount * 0.5f;
                data.refuel_speed *= 1 + (amount * 0.2f);

                ref var body = ref context.GetComponent<Body.Data>();
                if (!body.IsNull())
                {
                    body.mass_multiplier -= amount * 0.4f;
                }

                return true;
            },

            apply_0: static (ref Augment.Context context, ref Jetpack.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
            {

                ref var amount = ref handle.GetData<float>();
                ref var material_iron = ref Material.GetMaterial("iron_ingot");



                var ingot_amount = 0.00f;
                foreach (ref var requirement in context.requirements_new)
                {
                    if (requirement.type == Crafting.Requirement.Type.Work)
                    {
                        requirement.amount *= 1.25f;
                        requirement.difficulty += 4.00f;
                    }
                    else if (requirement.type == Crafting.Requirement.Type.Resource)
                    {
                        if (requirement.material.id == material_iron.id)
                        {
                            ingot_amount += requirement.amount;
                            requirement = default;
                        }
                    }
                }
                data.tank_type = Jetpack.TankType.Lightweight;
                var total_amount = ingot_amount * (1 - (amount * 0.4f));
                context.requirements_new.Add(Crafting.Requirement.Resource("iron_ingot", total_amount));
            }
        ));
    }
}