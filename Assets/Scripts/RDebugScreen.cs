using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RDebugScreen : MonoBehaviour
{
    World world;
    Text text; 

    float frameRate;
    float timer;

    // Start is called before the first frame update
    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "X - " + world.player.position.x + "\n";
        text.text += "Y - " + world.player.position.y + "\n";
        text.text += "Z - " + world.player.position.z + "\n\n";
        text.text += frameRate + " fps";

        if(timer > 1f)
        {
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
        }

    }
}
