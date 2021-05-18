using Grenades;
using UnityEngine;
using System;

namespace Vigilance.Custom.Items.Handling
{
    public class CollisionHandler : MonoBehaviour
    {
        public GameObject Owner { get; private set; }
        public Grenade Grenade { get; private set; }

        public void Init(GameObject owner, Grenade grenade)
        {
            Owner = owner;
            Grenade = grenade;
        }

        private void OnCollisionEnter(Collision collision)
        {
            try
            {
                if (collision.gameObject == Owner || collision.gameObject.TryGetComponent<Grenade>(out _))
                    return;

                Grenade.NetworkfuseTime = 0.1f;
            }
            catch (Exception exception)
            {
                Log.Error($"{nameof(OnCollisionEnter)} error:\n{exception}");
            }
        }
    }
}
