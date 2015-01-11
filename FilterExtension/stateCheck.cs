using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace FilterExtensions
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class stateCheck : MonoBehaviour
    {
        void Start()
        {
            StartCoroutine(checkState());
        }

        IEnumerator checkState()
        {
            // wait until the part menu is initialised
            while (!PartCategorizer.Ready)
                yield return new WaitForSeconds(1f);

            // wait for any other operations to finish
            yield return new WaitForSeconds(3f);

            // find out what happened with the startup
            if (EditorLogic.fetch != null)
            {
                if (Core.state == -1)
                {
                    Core.Log("Filter creation successful"); // filter creation ran to completion successfully
                    yield break;
                }
                else if (Core.state == 0)
                {
                    Core.Log("Filter creation restarting");
                    Core.Instance.editor(); // filter creation never started, lets try again
                    yield break;
                }
                else if (Core.state == 1)
                    Core.Log("Critical error encountered while creating filters"); // filter creation encountered a critical error, log it as such but don't attempt a retry
            }
        }
    }
}
