using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Seems.Modules.ContactForm;

[ApiController]
[Route("api/modules/contact-form")]
public class ContactFormController(ILogger<ContactFormController> logger) : ControllerBase
{
    /// <summary>
    /// Accepts a contact form submission from the public site.
    ///
    /// Validates required fields server-side using the <see cref="ContactFormSubmitRequest.FieldConfig"/>
    /// that the frontend echoes back from the slot parameters.
    ///
    /// Future extension points (already wired as comment stubs):
    ///   – Email notification via IEmailSender
    ///   – Persist submission to a ContactFormSubmission table
    /// </summary>
    [HttpPost("submit")]
    [AllowAnonymous]
    public IActionResult Submit([FromBody] ContactFormSubmitRequest request)
    {
        if (request.Fields is null || request.Fields.Count == 0)
            return BadRequest(new { message = "No form data provided." });

        // Server-side required-field validation
        var validationErrors = new Dictionary<string, string>();
        foreach (var field in request.FieldConfig.Where(f => f.Required))
        {
            if (!request.Fields.TryGetValue(field.Name, out var value) || string.IsNullOrWhiteSpace(value))
                validationErrors[field.Name] = $"{field.Label} is required.";
        }

        // Email format check for any field typed 'email'
        foreach (var field in request.FieldConfig.Where(f => f.Type == "email"))
        {
            if (request.Fields.TryGetValue(field.Name, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                if (!IsValidEmail(value))
                    validationErrors[field.Name] = "Please enter a valid email address.";
            }
        }

        if (validationErrors.Count > 0)
            return UnprocessableEntity(new { errors = validationErrors });

        // Log the submission (replace with email / DB persistence as needed)
        logger.LogInformation(
            "Contact form submission from page '{PageSlug}': {Fields}",
            request.PageSlug ?? "(unknown)",
            string.Join(", ", request.Fields.Select(kv => $"{kv.Key}={kv.Value}")));

        // TODO: inject IEmailSender and send notification to configured notifyEmail
        // TODO: persist to ContactFormSubmission entity for admin inbox view

        return Ok(new ContactFormSubmitResponse("Your message has been sent successfully."));
    }

    private static bool IsValidEmail(string email) =>
        System.Net.Mail.MailAddress.TryCreate(email, out _);
}
