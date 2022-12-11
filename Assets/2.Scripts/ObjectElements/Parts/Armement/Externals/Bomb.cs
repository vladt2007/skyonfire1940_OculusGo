﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : Part
{
    public override float EmptyMass() { return 0f; }
    public Mesh droppedMesh;
    public ExplosiveFiller filler;

    protected bool dropped = false;

    protected const float securityTimer = 1f;

    public virtual void Drop(float delayFuse, bool bay)
    {
        if (dropped) return;
        Detach();
        if (droppedMesh) GetComponent<MeshFilter>().sharedMesh = droppedMesh;
        rb.velocity += Random.insideUnitSphere * 0.4f;
        rb.inertiaTensor = new Vector3(emptyMass, emptyMass, emptyMass);
        dropped = true;
        GetComponent<Collider>().isTrigger = bay;
        GetComponent<Collider>().enabled = true;

        StartCoroutine(DropSequence(delayFuse));
    }
    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime)
        {
            dropped = false;
            GetComponent<Collider>().enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (dropped)
        {
            Vector3 forward = Vector3.RotateTowards(transform.forward, rb.velocity.normalized, Time.fixedDeltaTime * Mathf.PI * 2f / 10f, 0f);
            transform.rotation = Quaternion.LookRotation(forward, transform.up);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        StartCoroutine(TriggerOff());
    }
    private IEnumerator TriggerOff()
    {
        yield return new WaitForSeconds(0.1f);
        GetComponent<Collider>().isTrigger = false;
    }
    protected virtual IEnumerator DropSequence(float delayFuse)
    {
        bool safety = Mathf.Abs(rb.velocity.magnitude) < 15f;

        float securityCount = 0f;
        float heightPrediction;
        do
        {
            securityCount += Time.deltaTime;
            heightPrediction = transform.position.y - GameManager.map.HeightAtPoint(transform.position) + rb.velocity.y * Time.deltaTime;
            yield return null;
            
        } while (heightPrediction > 3f || Mathf.Abs(rb.velocity.y) < 1f && heightPrediction < 3f );

        if (rb.velocity.y < -10f) Root();            //Bomb is stuck in the ground
        if (securityCount < securityTimer && safety) //Safety Triggered, bomb will not explode
        {
            Destroy(transform.root.gameObject,20f);
            yield break;
        }
        yield return new WaitForSeconds(delayFuse);
        Detonate();
    }
    private void Bounce()
    {

    }
    protected void Root()
    {
        Vector3 pos = transform.position;
        pos.y = GameManager.map.HeightAtPoint(pos);
        transform.position = pos;
        GetComponent<Collider>().enabled = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.isKinematic = true;
    }
    protected void Detonate()
    {
        filler.Detonate(transform.position, emptyMass,null);
        Destroy(transform.root.gameObject);
    }
}
