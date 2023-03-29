﻿using UnityEngine;
using UnityEngine.Assertions;

namespace Assets
{
    public class Cell
    {
        private const string MaterialColorName = "_EmissionColor";
        private readonly GameObject Object;
        private readonly Material RendererMaterial;

        public CellType Type { get; private set; }

        public Cell(GameObject Object, CellType Type = CellType.EMPTY)
        {
            this.Object = Object;
            this.Type = Type;

            var Renderer = this.Object.GetComponent<Renderer>();
            Assert.IsNotNull(Renderer, "No renderer found on Cell Object.");
            Assert.IsNotNull(Renderer.material, "No material found on Cell Object Renderer.");

            this.RendererMaterial = new Material(Renderer.material);
            Renderer.material = this.RendererMaterial;
        }

        public void SetColor(Color Color)
        {
            this.RendererMaterial.SetColor(MaterialColorName, Color);
        }

        public void SetPosition(int X, int Y, float GridSpacing)
        {
            this.Object.transform.position = new Vector3(X * GridSpacing, Y * GridSpacing);
        }

        public void SetScale(float GridSpacing)
        {
            this.Object.transform.localScale = Vector3.one * GridSpacing;
        }
        
        public void SetTypeAndColor(CellType NewType, Color Color)
        {
            this.Type = NewType;
            this.SetColor(Color);
        }
    }

    // I went with this simple CellType enum structure, and not with any type of polymorphic behavior as this project is
    // too simple to justify using inheritance in my opinion.
    public enum CellType
    {
        EMPTY,
        SNAKE,
        APPLE
    }
}
