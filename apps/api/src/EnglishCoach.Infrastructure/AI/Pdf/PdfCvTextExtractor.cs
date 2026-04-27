using System.Text;
using System.Text.RegularExpressions;
using EnglishCoach.Application.Ports;
using UglyToad.PdfPig;

namespace EnglishCoach.Infrastructure.AI.Pdf;

public sealed class PdfCvTextExtractor : ICvTextExtractor
{
    public Task<string> ExtractTextAsync(Stream fileStream, CancellationToken ct = default)
    {
        try
        {
            using var document = PdfDocument.Open(fileStream);
            var builder = new StringBuilder();

            foreach (var page in document.GetPages())
            {
                ct.ThrowIfCancellationRequested();
                builder.AppendLine(page.Text);
            }

            return Task.FromResult(NormalizeWhitespace(builder.ToString()));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw new InvalidDataException(
                "Could not extract readable text from this PDF. Please paste your CV text instead.",
                exception);
        }
    }

    private static string NormalizeWhitespace(string text)
    {
        var normalizedLines = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n')
            .Select(line => Regex.Replace(line.Trim(), @"\s+", " "))
            .Where(line => line.Length > 0);

        return string.Join(Environment.NewLine, normalizedLines);
    }
}
