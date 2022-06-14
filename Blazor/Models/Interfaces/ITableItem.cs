using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace SSW.Rewards.Admin.Models.Interfaces;

public interface ITableItems
{
    public int Id { get; set; }
    public string Name { get; set; }
    public RenderFragment OverwriteRowRender(string rowName);
}

public class TableHeaders
{
    public string Heading { get; set; }
    public string? Label { get; set; }
    public bool Overwrite { get; set; } = false;
}