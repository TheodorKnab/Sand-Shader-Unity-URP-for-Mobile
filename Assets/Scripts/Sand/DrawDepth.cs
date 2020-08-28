using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;

public class DrawDepth : MonoBehaviour
{
    [SerializeField] private float blurSize;    
    [SerializeField] private float differenceBlurSize;    
    [SerializeField] private RenderTexture depthTex;
    [SerializeField] private RenderTexture addTexture;
    [SerializeField] private Material depthMaterial;
    [SerializeField] private Material disperseMaterial;
    [SerializeField] private Material differenceMaterial;
    [SerializeField] private Material blurXMaterial;
    [SerializeField] private Material blurYMaterial;
    
    
    [HideInInspector] public RenderTexture depthTextureNotBlured;
    private RenderTexture _oldDepthTexture;
    private RenderTexture _tempTexA;
    private RenderTexture _tempTexB;
#if UNITY_EDITOR
    private RenderTexture _debugTex;
#endif



    void Awake()
    {
        RenderTextureDescriptor texDescriptor = new RenderTextureDescriptor(1024,1024, GraphicsFormat.R8_UNorm,0,0);
        depthTextureNotBlured = new RenderTexture(texDescriptor);
        _oldDepthTexture = new RenderTexture(texDescriptor);
        _tempTexA = new RenderTexture(texDescriptor);
        _tempTexB = new RenderTexture(texDescriptor);
        
#if UNITY_EDITOR
        _debugTex = new RenderTexture(texDescriptor);
#endif
        
        //set entire depth map to 0.5 on start
        Texture2D tex = new Texture2D(depthTex.width, depthTex.height, TextureFormat.R8, false);
        Color fillColor  = new Color(0.5f, 0.5f, 0.5f);
        var fillColorArray =  tex.GetPixels();
        for(var i = 0; i < fillColorArray.Length; ++i)
        {
            fillColorArray[i] = fillColor;
        }
        tex.SetPixels( fillColorArray );
        tex.Apply();
        Graphics.Blit(tex, depthTextureNotBlured);
    }

    void Update()
    {
        //Get the difference of the depth texture to the previous frame
        differenceMaterial.SetTexture("_OldTex", _oldDepthTexture);
        differenceMaterial.SetTexture("_NewTex", addTexture);
        Graphics.Blit(addTexture, _tempTexB, differenceMaterial);
        Graphics.Blit(addTexture, _oldDepthTexture);

        //Blur this difference
        blurXMaterial.SetFloat("_Size", differenceBlurSize);
        blurYMaterial.SetFloat("_Size", differenceBlurSize);

        blurXMaterial.SetTexture("_MainTex", _tempTexB);
        Graphics.Blit(_tempTexB, _tempTexA, blurXMaterial);
        blurYMaterial.SetTexture("_MainTex", _tempTexA);
        Graphics.Blit(_tempTexA, _tempTexB, blurYMaterial);
        
        //Blured difference is used to create a dispersion effect (sand buildup in the movement direction)
        //Gets applied to the existing depth texture
        disperseMaterial.SetTexture("_SubTex", _tempTexB);
        disperseMaterial.SetTexture("_DepthTex", depthTextureNotBlured);
        Graphics.Blit(_tempTexB, _tempTexA, disperseMaterial);

        //Apply current depth to the existing depth texture
        depthMaterial.SetTexture("_AddTex", addTexture);
        depthMaterial.SetTexture("_DepthTex", _tempTexA);
        Graphics.Blit(addTexture, _tempTexB, depthMaterial);
        Graphics.Blit(_tempTexB, depthTextureNotBlured); //save a non blured version of this depth texture

        //Blur the entire texture
        blurXMaterial.SetFloat("_Size", blurSize);
        blurYMaterial.SetFloat("_Size", blurSize);
        
        blurXMaterial.SetTexture("_MainTex", depthTextureNotBlured);
        Graphics.Blit(depthTextureNotBlured, _tempTexA, blurXMaterial);
        
        blurYMaterial.SetTexture("_MainTex", _tempTexA);
        Graphics.Blit(_tempTexA, depthTex, blurYMaterial);
        
        //blurYMaterial.SetTexture("_MainTex", _tempTexB);
        //Graphics.Blit(_tempTexB, _tempTexA, blurYMaterial);
        //
        //blurXMaterial.SetTexture("_MainTex", _tempTexA);
        //Graphics.Blit(_tempTexA, _tempTexB, blurXMaterial);
        //
        //blurYMaterial.SetTexture("_MainTex", _tempTexB);
        //Graphics.Blit(_tempTexB, depthTex, blurYMaterial);
    }

    private void OnDestroy()
    {
        depthTextureNotBlured.Release();
        _oldDepthTexture.Release();
        _tempTexA.Release();
        _tempTexB.Release();

    }
}
