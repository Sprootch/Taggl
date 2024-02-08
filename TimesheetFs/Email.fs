module Email

open System
open Microsoft.Office.Interop.Outlook

let openEmail date attachment =
    let mail = ApplicationClass().CreateItem(OlItemType.olMailItem) :?> MailItem
    mail.Subject <- $"Timesheet {date:Y}"
    mail.To <- "pnijs@actiris.be; schauvaux@actiris.be"

    mail.Attachments.Add(attachment, OlAttachmentType.olByValue, Type.Missing, Type.Missing)
    |> ignore

    let body =
        $"""
         <p>Bonjour,</p>
         <p>Voici ma Timesheet pour le mois de {date:Y}.</p>
         Cordialement,
         """

    mail.Recipients.ResolveAll() |> ignore
    mail.Display(false)

    // This is done after opening the mail to have the default signature.
    mail.HTMLBody <- body + mail.HTMLBody

    ()
