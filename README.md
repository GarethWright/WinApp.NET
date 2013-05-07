WinApp.NET
==========
![WinApp.NET](https://dl.dropboxusercontent.com/u/68235039/winapi.png)

Simple WhatsApp chat client using .NET 4 and WhatsApiNet

USAGE:
You need an existing WhatsApp account with a valid password.
Enter your credentials in app.config

This program uses SQLite to store contacts and messages.

This program also uses Google contacts import.
Just enter your email and password in the dialog and all contacts having phonenumbers will be imported.
The application will sync the contacts with WhatsApp servers to check which are valid WhatsApp contacts.

Currently working on:
- Group chats
- Change your profile (status, nickname & profile picture)
- Receiving media files
- Add credits for used libraries (in /Libraries)
- Custom style in list views (chats, conversation, contacts)

To-do:
- Full-sized profile pictures (700px*700px, only 100px*100px previews are used at the moment)
- Sending media files
- Import contacts from other services
- MSI installer
- Login screen (whatsapp & google credentials input)
- Add registration screen (voice & sms) and credentials check
- Desktop notifications
- Unified layout (all views in single window using table layout and tabs for chat windows)
- Skinning
