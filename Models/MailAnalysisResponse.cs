using MimeKit;

namespace Models;
public class MailAnalysisResponse
{
    public long orderId { get; set; }
    public long analysisId { get; set; }
    public byte[] attachment { get; set; }
}
