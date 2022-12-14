namespace SSW.Rewards.Controls;

public class RewardsProgressBar : IDrawable
{
    private readonly float _progress;

    public RewardsProgressBar()
    {
        _progress = 0f;
    }

    public RewardsProgressBar(float progress)
    {
        _progress = progress;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.StrokeSize = 60;
        canvas.StrokeColor = Color.FromRgb(52, 52, 52);

        var radius = (dirtyRect.Width * 0.75f) / 2; // this will make the circle take up 3/4 the width

        canvas.DrawCircle(dirtyRect.Width / 2, dirtyRect.Height / 2, radius);

        var rectTop = (dirtyRect.Height / 2) - radius;
        var rectBottom = (dirtyRect.Height / 2) + radius;
        var rectRight = (dirtyRect.Width / 2) + radius;
        var rectLeft = (dirtyRect.Width / 2) - radius;

        canvas.StrokeColor = Color.FromRgb(204, 65, 65);

        canvas.DrawArc(rectLeft, rectTop, rectRight, rectBottom, 0, _progress, true, false);
    }
}