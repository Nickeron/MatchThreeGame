using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : GamePiece
{
    public bool clearedByBomb = false, clearedAtBottom = true;

    // Start is called before the first frame update
    void Start()
    {
        matchValue = MatchValue.None;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
