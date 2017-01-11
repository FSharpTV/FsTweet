module FsTweet.Web.EmailService
open System.Net
open System.Net.Mail
open FsTweet.Domain.User
open System

type SmtpConfig = {
  Username : string
  Password : string
  Host : string 
  Port : int
}

type Email = {
  Subject : string
  From : string
  Destination : string
  Body : string
  IsBodyHtml : bool
}

let sendEmail onEmailSent config email =  
  
  use mail = 
    new MailMessage
      ( email.From, 
        email.Destination, 
        email.Subject, 
        email.Body, 
        IsBodyHtml = email.IsBodyHtml )

  let clientCredentials = new NetworkCredential(config.Username, config.Password)
  
  let smtpClient = 
    new SmtpClient
      ( config.Host,
        config.Port, 
        EnableSsl = true,
        UseDefaultCredentials = false,
        Credentials = clientCredentials )

  smtpClient.SendCompleted.Add onEmailSent
  smtpClient.SendAsync(mail, null)

let sendActivationEmail activationUrl sendEmail (user : User) = 
  let emailTemplate (userName:Username) (link:Uri) = 
    sprintf 
      """
      Hi %s,   
      Your FsTweet account has been created successfully.   
      <a href="%O"> Click here </a> to activate your account.
      
      Regards
      FsTweet""" userName.Value link

  sendEmail
   { Subject = "Your FsTweet account has been created"
     From = "email@fstweet.com"
     Destination = user.EmailAddress.Value
     Body = emailTemplate user.Username activationUrl
     IsBodyHtml = true } 
