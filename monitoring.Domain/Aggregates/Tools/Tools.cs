using Domain.Aggregates.Tools.ValueObjects;
using Monitoring.Domain.SeedWork;
using Domain.SharedKernel;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitoring.Domain.Aggregates.Tools;

public class Tool : AggregateRoot
{
    public string Name { get; private set; }
    public string DefaultJs { get; private set; }
    public string ElementType { get; private set; }
    public List<Template> Templates { get; private set; }
    public List<Asset> DefaultAssets { get; private set; }

    private Tool() { }

    private Tool(string name, string defaultJs, string elementType, List<Template> templates, List<Asset> defaultAssets)
    {
        Name = name;
        DefaultJs = defaultJs;
        ElementType = elementType;
        Templates = templates;
        DefaultAssets = defaultAssets;
    }

    public static Result<Tool> Create(string name, string defaultJs, string elementType, List<Template> templates, List<Asset> defaultAssets)
    {
        var result = new Result<Tool>();

        if (string.IsNullOrWhiteSpace(name))
        {
            result.WithError("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(elementType))
        {
            result.WithError("ElementType is required.");
        }

        if (templates == null || templates.Count == 0)
        {
            result.WithError("At least one template is required.");
        }

        if (result.IsFailed)
        {
            return result;
        }

        var tool = new Tool(name, defaultJs, elementType, templates, defaultAssets);
        result.WithValue(tool);

        return result;
    }
    public void Update(string name, string defaultJs, string elementType, List<Template> templates, List<Asset> defaultAssets)
    {
        Name = name;
        DefaultJs = defaultJs;
        ElementType = elementType;
        Templates = templates;
        DefaultAssets = defaultAssets;

    }

}

