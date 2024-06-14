==========
 Setup
==========
1. Retrieve client secret value from Azure app registrations/Certificates & secrets
2. Set app register under user's environment variable
	- update into secret.txt
3. Please take note that client secret value only valid for 6 months, developer needs to update upon maturity date
4. Retrieve Tenant ID and Cient ID from Azure app registrations/Overview
	- update into appsettings.json
5. Retrieve Drive ID via Microsoft Graph Explorer [https://developer.microsoft.com/en-us/graph/graph-explorer]
	1. At query URL type 'https://graph.microsoft.com/v1.0/me/drive/root'
	2. Set method to 'GET' and version 'V1.0'
	3. Hit run query and retieve value associated with key '/parentReference/driveId'
	4. Update drive ID into appsettings.json

======================
Command line example:
======================
-upload "C:\Users\benjamin\Downloads\testing graph.pdf" "My Share Folder/Backup/ssl/account summary.pdf"
-download "C:\Users\benjamin\Downloads\testing graph.pdf" "My Share Folder/Backup/ssl/account summary.pdf"

=====================
Deployment - Linux
=====================
- Must deploy with following settings
	- target framwork: net6.0
	- deployment mode: self-contained
	- taget runtime: linux-x64
	- producr single file: false
- make sure curl is installed on Linux machine
- change folder group owner to Apache2
	sudo chgrp -R www-data /var/www/erp