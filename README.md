WinApp.NET
==========

Simple WhatsApp chat client using .NET 4 and WhatsApiNet

USAGE:
You need an existing whatsapp account with a valid password.
Enter your credentials in ContactsList.cs

This program also supports Google contact import.
Uncomment these lines in ContactsList.cs

    //ContactStore.SyncGoogleContacts("youremail@gmail.com", "passw3rd");
    //ContactStore.SyncWaContacts(this.username, this.password);
and enter your google credentials (full email + password)
The first line will download and insert all Google contacts with phone numbers.
The second line will sync all stored contacts with WhatsApp and remove contacts who don't have a WhatsApp account.

TODO:
- Login screen (whatsapp & google credentials input)
- Add contact sync button (manual sync instead of startup sync)
- Fix image receiving (problem in WhatsApiNet)
- Add contact image
- Custom style in list views (chats, conversation, contacts)
- Add registration screen (voice & sms) and credentials check
- Add credits for used libraries (in /Libraries)
- Different logo (using the WhatsApp logo ATM, could get me in trouble)
- Desktop notifications
- Unified layout (all views in single window using table layout and tabs for chat windows)
