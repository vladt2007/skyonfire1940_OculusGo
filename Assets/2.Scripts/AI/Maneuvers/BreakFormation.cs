using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakFormation : ActiveManeuver
{
    private SofAircraft leader;
    private Vector3 breakDir;
    private float breakCountDown;
    public override void Initialize(AI.GeometricData data)
    {
        base.Initialize(data);
        leader = GameManager.squadrons[data.aircraft.squadronId][0];
        breakDir = aircraft.card.formation.GetBreakDirection(leader.transform, aircraft.placeInSquad);
        breakCountDown = 2.5f;
    }
    public override string Label()
    {
        return "Break Formation";
    }
    public override void Execute(AI.GeometricData data)
    {
        if (done) return;
        base.Execute(data);

        breakCountDown -= Time.fixedDeltaTime;
        Vector3 axis = AircraftControl.TrackingInputs(transform.position + breakDir * 300f, aircraft, 0f, 0f, true);
        aircraft.SetControls(axis, true, false);
        if (breakCountDown < 0f) done = true;
    }
}
