using Content.Server.Heretic.Components;
using Content.Server.Popups;
using Content.Shared.DoAfter;
using Content.Shared.Heretic;
using Content.Shared.Interaction;

namespace Content.Server.Heretic;

public sealed partial class EldritchInfluenceSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doafter = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly HereticSystem _heretic = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EldritchInfluenceComponent, InteractHandEvent>(OnInteract);
        SubscribeLocalEvent<EldritchInfluenceComponent, EldritchInfluenceDoAfterEvent>(OnDoAfter);
    }

    public void OnInteract(Entity<EldritchInfluenceComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled)
            return;

        if (!HasComp<HereticComponent>(args.User))
            return;

        if (ent.Comp.Spent)
            return;

        var dargs = new DoAfterArgs(EntityManager, args.User, 10f, new EldritchInfluenceDoAfterEvent(), args.Target, args.Target)
        {
            Hidden = true,
            BreakOnHandChange = true,
            BreakOnMove = true,
            BreakOnDamage = true,
            CancelDuplicate = true
        };
        _popup.PopupEntity(Loc.GetString("heretic-influence-start"), ent, ent);
        _doafter.TryStartDoAfter(dargs);

        args.Handled = true;
    }
    public void OnDoAfter(Entity<EldritchInfluenceComponent> ent, ref EldritchInfluenceDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        if (args.Target == null)
            return;

        if (!TryComp<HereticComponent>(args.User, out var heretic))
            return;

        _heretic.UpdateKnowledge(args.User, heretic, heretic.CodexActive ? 2 : 1);

        Spawn("EldritchInfluenceSpent", Transform((EntityUid) args.Target).Coordinates);
        QueueDel(args.Target);
    }
}
