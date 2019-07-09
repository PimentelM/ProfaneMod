using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;


public class GUIVersionFiller : UnityEngine.MonoBehaviour
{

    private void Awake()
    {
        Mod.Loader.Load();
    }

}

