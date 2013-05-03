WinApp.NET
==========

Simple WhatsApp chat client using .NET 4 and WhatsApiNet

USAGE:
You need an existing whatsapp account with a valid password.
Enter your credentials in app.config

This program uses SQLite to store contacts and messages.

This program also uses Google contacts import on startup.
Just enter your email and password in the dialog and all contacts having phonenumbers will be imported.
The application will sync the contacts with WhatsApp servers to check which are valid WhatsApp contacts.

Currently working on:
- Add Google contact sync button (manual sync instead of startup sync)
- Fix image receiving (problem in WhatsApiNet)
- Add contact image

Pending:
- Login screen (whatsapp & google credentials input)
- Custom style in list views (chats, conversation, contacts)
- Add registration screen (voice & sms) and credentials check
- Add credits for used libraries (in /Libraries)
- Different logo (using the WhatsApp logo ATM, could get me in trouble)
- Desktop notifications
- Unified layout (all views in single window using table layout and tabs for chat windows)
