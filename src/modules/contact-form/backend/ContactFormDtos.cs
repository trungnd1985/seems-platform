namespace Seems.Modules.ContactForm;

/// <summary>
/// A single field definition as configured by the admin in the slot parameters.
/// </summary>
public class FieldConfig
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = "text";
    public bool Required { get; set; }
}

/// <summary>
/// Incoming payload from the public contact form submission.
/// </summary>
public class ContactFormSubmitRequest
{
    /// <summary>
    /// Slug of the page that hosted the form — useful for routing
    /// email notifications or future multi-page inbox support.
    /// </summary>
    public string? PageSlug { get; set; }

    /// <summary>
    /// Dynamic field values keyed by the field name defined in the slot parameters.
    /// e.g. { "name": "John", "email": "j@example.com", "message": "Hello" }
    /// </summary>
    public Dictionary<string, string> Fields { get; set; } = [];

    /// <summary>
    /// The field definitions that were active when the form was submitted.
    /// Used server-side to enforce required-field validation independently
    /// of the frontend.
    /// </summary>
    public List<FieldConfig> FieldConfig { get; set; } = [];
}

/// <summary>Response returned on successful submission.</summary>
public record ContactFormSubmitResponse(string Message);
