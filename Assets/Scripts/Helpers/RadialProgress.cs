using UnityEngine;
using UnityEngine.UIElements;

namespace Helpers
{
    [UxmlElement]
    public partial class RadialProgress : VisualElement
    {
       float progress;
       Color fillColor = new Color(1, 1, 1, 0.5f);
       bool clockwise = true;
       bool isSquare;

       [UxmlAttribute]
       public float Progress
       {
          get => progress;
          set
          {
             progress = Mathf.Clamp01(value);
             MarkDirtyRepaint();
          }
       }

       [UxmlAttribute]
       public Color FillColor
       {
          get => fillColor;
          set
          {
             fillColor = value;
             MarkDirtyRepaint();
          }
       }

       [UxmlAttribute]
       public bool Clockwise
       {
          get => clockwise;
          set
          {
             clockwise = value;
             MarkDirtyRepaint();
          }
       }

       [UxmlAttribute]
       public bool IsSquare
       {
          get => isSquare;
          set
          {
             isSquare = value;
             MarkDirtyRepaint();
          }
       }

       public RadialProgress()
       {
          style.flexGrow = 1;
          style.overflow = Overflow.Hidden; 
          
          generateVisualContent += OnGenerateVisualContent;
       }

       void OnGenerateVisualContent(MeshGenerationContext ctx)
       {
          switch (progress)
          {
             case <= 0f:
                return;
             case >= 0.999f:
                DrawFullFill(ctx);
                return;
          }

          Painter2D painter = ctx.painter2D;
          painter.fillColor = fillColor;

          float width = contentRect.width;
          float height = contentRect.height;
          Vector2 center = new Vector2(width / 2f, height / 2f);

          float radius;
          if (isSquare)
          {
             // We deliberately calculate a radius larger than the box 
             // to ensure the arc covers the corners.
             // The Overflow.Hidden in the constructor clips the excess.
             float halfSide = Mathf.Max(width, height) / 2f;
             radius = Mathf.Sqrt(2) * halfSide;
          }
          else
          {
             radius = Mathf.Min(width, height) / 2f;
          }

          const float startAngle = -90f;
          float totalAngle = progress * 360f;
          float endAngle;
          ArcDirection arcDirection;

          if (clockwise)
          {
             endAngle = startAngle + totalAngle;
             arcDirection = ArcDirection.Clockwise;
          }
          else
          {
             endAngle = startAngle - totalAngle;
             arcDirection = ArcDirection.CounterClockwise;
          }

          painter.BeginPath();
          painter.MoveTo(center);
          painter.Arc(center, radius, startAngle, endAngle, arcDirection);
          painter.LineTo(center);
          painter.ClosePath();
          painter.Fill();
       }

       void DrawFullFill(MeshGenerationContext ctx)
       {
          Painter2D painter = ctx.painter2D;
          painter.fillColor = fillColor;

          float width = contentRect.width;
          float height = contentRect.height;

          if (isSquare)
          {
             painter.BeginPath();
             painter.MoveTo(new Vector2(0, 0));
             painter.LineTo(new Vector2(width, 0));
             painter.LineTo(new Vector2(width, height));
             painter.LineTo(new Vector2(0, height));
             painter.ClosePath();
          }
          else
          {
             float radius = Mathf.Min(width, height) / 2f;
             Vector2 center = new Vector2(width / 2f, height / 2f);

             painter.BeginPath();
             painter.Arc(center, radius, 0, 360);
          }

          painter.Fill();
       }
    }
}