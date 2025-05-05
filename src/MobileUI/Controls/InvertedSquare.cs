using Maui.BindableProperty.Generator.Core;

namespace SSW.Rewards.Mobile.Controls;

public partial class InvertedSquare : BindableObject, IDrawable
{
    [AutoBindable]
    private readonly float _squareSize = 200f;

    [AutoBindable]
    private readonly float _cornerRadius = 0f;

    [AutoBindable]
    private readonly Color _backgroundColor = Colors.Black;

    [AutoBindable]
    private readonly float _opacity = 1f;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float centerX = dirtyRect.Width / 2;
        float centerY = dirtyRect.Height / 2;
        RectF transparentSquare = new RectF(
            centerX - SquareSize / 2,
            centerY - SquareSize / 2,
            SquareSize,
            SquareSize);

        var path = new PathF();
        path.AppendRectangle(dirtyRect);
        path.AppendRoundedRectangle(transparentSquare, CornerRadius);
        path.Close();

        var adjustedColor = BackgroundColor.WithAlpha(Opacity);
        canvas.FillColor = adjustedColor;
        canvas.FillPath(path, WindingMode.EvenOdd);
    }
}