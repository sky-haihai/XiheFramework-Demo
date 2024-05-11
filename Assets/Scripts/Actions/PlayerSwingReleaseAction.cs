using System;
using Constants;
using PlayerEntities;
using UnityEngine;
using XiheFramework.Combat.Action;
using XiheFramework.Core.Utility.Extension;
using XiheFramework.Runtime;

namespace Actions {
    public class PlayerSwingReleaseAction : TennisPlayerActionEntityBase {
        public override string EntityAddressName => PlayerActionNames.PlayerSwingRelease;

        public Material hitMaterial;
        public LayerMask hitLayerMask;

        protected override void OnActionInit() {
            base.OnActionInit();
            var swingDir = FetchArgument<Vector2>(ActionArgumentNames.SwingDirection);
            var swingPower = FetchArgument<float>(ActionArgumentNames.SwingPower);

            var isHit = Physics.SphereCast(owner.transform.position - swingDir.ToVector3(V2ToV3Type.XY) * owner.swingRadius / 2, owner.swingRadius / 2,
                swingDir.ToVector3(V2ToV3Type.XY), out var sphereHit,
                owner.swingRadius, hitLayerMask);

            // var isHit = Physics.Raycast(owner.transform.position, swingDir.ToVector3(V2ToV3Type.XY), out var sphereHit, owner.swingRadius, hitLayerMask);
            if (isHit) {
                Debug.Log("Hit at" + sphereHit.point);
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.transform.position = sphereHit.point;
                go.transform.localScale = Vector3.one * 0.1f;
                go.GetComponent<Renderer>().material = hitMaterial;
                Destroy(go.gameObject, 1f);

                var deltaDir = sphereHit.point - owner.transform.position;
                var distance = Vector3.Distance(owner.transform.position, sphereHit.point);
                distance = Mathf.Clamp(distance, 0, owner.swingRadius);
                var force = swingDir.ToVector3(V2ToV3Type.XY) * (Mathf.Lerp(0, 5, (owner.swingRadius - 0) / owner.swingRadius) * owner.power);

                var rb = sphereHit.collider.GetComponent<Rigidbody>();
                if (rb != null) {
                    //replace 0 to distance
                    rb.velocity = Vector3.zero;
                    rb.AddForceAtPosition(force, sphereHit.point, ForceMode.VelocityChange);
                }

                if (Physics.Raycast(sphereHit.point, -deltaDir, out var hitOnOwner, owner.swingRadius)) {
                    owner.rigidBody.velocity = Vector3.zero;
                    owner.rigidBody.AddForceAtPosition(-force, hitOnOwner.point, ForceMode.Impulse);
                }
                else {
                    owner.rigidBody.velocity = Vector3.zero;
                    owner.rigidBody.AddForceAtPosition(-force, owner.transform.position, ForceMode.Impulse);
                }

                Game.LogicTime.SetGlobalTimeScaleInSecond(0.05f, 1f, true);
            }
            else {
                owner.rigidBody.AddForceAtPosition(-swingDir * 0.1f, owner.transform.position, ForceMode.Impulse);
            }
        }

        protected override void OnActionUpdate() {
            ChangeAction(PlayerActionNames.PlayerIdle);
        }

        protected override void OnActionExit() { }

        private void OnDrawGizmos() {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(owner.transform.position, owner.swingRadius);
        }
    }
}