
namespace TC2.NukesMod;

public sealed partial class ModInstance: Mod
{
	protected override void OnRegister(ModContext context)
	{
		Augment.OnInitialize += RegisterJetpackAugments;
	}

	protected override void OnInitialize(ModContext context)
	{
			
	}

	protected override void OnConfigure(ModContext context)
	{

	}
}