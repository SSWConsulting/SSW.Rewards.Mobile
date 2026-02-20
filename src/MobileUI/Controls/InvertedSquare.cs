using CommunityToolkit.Maui;

namespace SSW.Rewards.Mobile.Controls;

public partial class InvertedSquare : BindableObject, IDrawable
{
    [BindableProperty]
    public partial float SquareSize { get; set; } = 200f;
    
    [BindableProperty]
    public partial float CornerRadius { get; set; } = 0f;
    
    [BindableProperty]
    public partial Color BackgroundColor { get; set; } = Colors.Black;
    
    [BindableProperty]
    public partial float Opacity { get; set; } = 1f;

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