# POC-Docviewer
POC for Document Viewer
The code can be downloaded from below mentioned location
https://github.com/manjeetmedefer/POC-Docviewer

The POC is having two Approaches:
1. Ope File from DB - For this user has to click on "Uplaod controller". This will allow the user to upload the file to SQL DB configured 
          in the Medefer code.
          Once the file is uploaded, there is an link to that document.
          On click of that link the Uploaded file will be opened in New Docviewer control from SQL DB.
       
2. Open file from local - There is a link "View " which is provided in the home page of application under the section.
          On click of that link the file will be streamed from the local FTP on to new Docviewer control.
          

The objective of this POC is to compare the performance between the two approaches provided above.

There is an login mechenism which will make a log of time stamp for the differnt stages of opening the document by both the approaches

It basically covers: [EXAMPLE]
    Before read from Local file - 5/4/2020 12:08:12 PM
    After read from local file- 5/4/2020 12:08:14 PM
    Before Render - 5/4/2020 12:08:14 PM
    After Render - 5/4/2020 12:08:17 PM

The logs will be generated and stored in below mentioned location\folder:
  [project Location]\DocumentManager\DocumentManager\documents\Logs
    
