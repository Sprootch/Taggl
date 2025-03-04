module Email

open System
open System.Globalization
open Microsoft.Office.Interop.Outlook

let openEmail recipients (date: DateTime) attachment =
    let mail = ApplicationClass().CreateItem(OlItemType.olMailItem) :?> MailItem
    let date = date.ToString("Y", CultureInfo("FR"))
    mail.Subject <- $"Timesheet {date}"
    mail.To <- recipients

    mail.Attachments.Add(attachment, OlAttachmentType.olByValue, Type.Missing, Type.Missing)
    |> ignore

    let body =
        $"""
         <p>Bonjour,</p>
         <p>Voici ma Timesheet pour le mois de {date}.</p>
         Cordialement,
         """

    mail.Recipients.ResolveAll() |> ignore
    mail.Display(false)

    // This is done after opening the mail to have the default signature.
    mail.HTMLBody <- body + mail.HTMLBody

    ()
