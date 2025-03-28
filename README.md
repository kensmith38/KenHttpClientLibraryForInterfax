<h1>KenHttpClientLibraryForInterfax is a library for sending faxes using Interfax's REST API. </h1>

**YouTube video**<br> This video demonstrates the functionality of KenHttpClientLibraryForInterfax.<br>
https://youtu.be/3xHhg4TFuqc

**Design:** <br>
1. Ken used a lot of the code, techniques, and concepts in Interfax's official Library (InterFAX.NET) which is found on GitHub.<br>
2. KenHttpClientLibraryForInterfax supports only "outbound" faxes (and "batches"); there is no support for "inbound" faxes.<br>
3. KenHttpClientLibraryForInterfax does not support sending multiple documents in a single fax.<br>
4. KenHttpClientLibraryForInterfax supports faxing any file up to 20 MB in size.<br>
4a. Files larger than 8 MB are uploaded in chunks automatically (transparent to the user).<br>
4b. The actual limit is specified in the Settings.cs file (ThresholdToUploadInChunks)<br>
5. KenHttpClientLibraryForInterfax retrieves many more details than InterFAX.NET library which retrieves a summary of details.<br>
6. Ken's production application stores faxIDs (and batchIDs) in a database as faxes (and batches) are sent.<br>
6a. KenHttpClientLibraryForInterfax does not include extensive search options because Ken's database has all the faxIDs and batchIDs and
    that information is sufficient to get details for any fax.<br>

**Summary:** KenHttpClientLibraryForInterfax is almost a subset of the InterFAX.NET library with the following differences:<br>
1. KenHttpClientLibraryForInterfax can send a fax to multiple destinations (batch)! <br>
2. KenHttpClientLibraryForInterfax supports outbound faxes (but not inbound).<br>
3. KenHttpClientLibraryForInterfax automatically uses "document upload" for large faxes (files larger than 8 Mb; actually ThresholdToUploadInChunks).<br>
3a. It is very convenient that a file of any size can be faxed using the "SendFax*" method without worrying about its size.<br>

**Caveats:**<br>
1. While this code appears to function correctly based on initial testing, it has not been thoroughly tested under all possible scenarios,
   and therefore, there's a possibility of unexpected behavior or errors in production.

**Companion program:**<br>
    KenHttpClientTestAppForInterfax also exists on GitHub.  It is a C# .Net Framework app to test KenHttpClientLibraryForInterfax.
