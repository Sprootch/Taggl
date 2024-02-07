using Microsoft.Office.Interop.Outlook;

PrepareOutlookMail(DateTime.Today, @"c:\temp\TS-202401-Delcoigne-Vincent.xlsx");

void PrepareOutlookMail(DateTime date, string attachmentPath)
{
    var mail = (MailItem) new Application().CreateItem(OlItemType.olMailItem);
    mail.Subject = $"Timesheet {date:Y}";
    var recipTo = mail.Recipients.Add("pnijs@actiris.be");
    recipTo.Type = (int) OlMailRecipientType.olTo;
    var recipCc = mail.Recipients.Add("schauvaux@actiris.be");
    recipCc.Type = (int) OlMailRecipientType.olTo;

    mail.Attachments.Add(attachmentPath,
        OlAttachmentType.olByValue, Type.Missing, Type.Missing);

    var body =
        $"""
         <p>Bonjour,</p>
         <p>Voici ma Timesheet pour le mois de {date:Y}.</p>
         Cordialement,
         """;
    mail.Recipients.ResolveAll();
    mail.Display(false);
    mail.HTMLBody = body + mail.HTMLBody;
}
