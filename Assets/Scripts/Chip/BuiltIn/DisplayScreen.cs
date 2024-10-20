using System.Drawing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using DLS.Core.Simulation;
using UnityEngine;
using Color = UnityEngine.Color;
using Debug = UnityEngine.Debug;

public class DisplayScreen : BuiltinChip
{
    public Renderer textureRender;
    public const int SIZE = 8;
    private int editCoords;
    Texture2D texture;
    int[] texCoords;
    
    public override void Init()
    {
        base.Init();
        ChipType = ChipType.Miscellaneous;
        PackageGraphicData = new PackageGraphicData()
        {
            PackageColour = new UnityEngine.Color(82, 17, 78, 255),
        };
        inputPins = new List<Pin>(12);
        outputPins = new List<Pin>();
        Name = "DISP8";
        
    }
    

    public static Texture2D CreateSolidTexture2D(UnityEngine.Color color, int width, int height = -1) {
        if(height == -1) {
            height = width;
        }
        Texture2D texture = new Texture2D(width, height);
        UnityEngine.Color[] pixels = Enumerable.Repeat(color, width * height).ToArray();
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    public int[] map2d(int index, int size) {
        var coords = new int[2];
        coords[0] = index % size;
        coords[1] = index / size;
        return coords;
    }

	protected override void Awake()
	{
        texture = CreateSolidTexture2D(new UnityEngine.Color(0, 0, 0), SIZE);
        texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;
        textureRender.sharedMaterial.mainTexture = texture;
		base.Awake();
	}

    //update display here
    public override void ProcessOutput() {

        PinStates AdressPins = new PinStates(Pin.WireType.Simple);
        for(int i = 11; i>5 ; i--)
            AdressPins.Add(inputPins[i].State[0]);;

        var cord =(int) AdressPins.ToUInt();

        texCoords = map2d(cord, SIZE);
        var redChannel = (inputPins[0].State.ToUInt() + inputPins[1].State.ToUInt())/ 2f;
        var greenChannel = (inputPins[2].State.ToUInt() + inputPins[3].State.ToUInt()) / 2f;
        var blueChannel = (inputPins[4].State.ToUInt() + inputPins[5].State.ToUInt()) / 2f;
        texture.SetPixel(texCoords[0], texCoords[1], new Color(redChannel , greenChannel, blueChannel));
        texture.Apply();
    }


}
