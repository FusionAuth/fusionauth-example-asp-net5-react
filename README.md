# Example ASP.NET Core React SPA Application

An ASP.NET Core React SPA application using FusionAuth as the identity server. This application will use a OAuth Authorization Code workflow to log users in.

You need to have dotnetcore & npm installed to run this code. Please note that this application uses .net 5.0.

Set up FusionAuth as documented in the blog post.

Windows Install
To deploy and run on Windows, assuming you have the dotnet 5.0 runtime installed:

Assuming the files are located under `c:\SampleApp`
- Open the project in your favourite editor
- Update appsettings.json with the FusionAuth application ClientId, ClientSecret, ApiKey and the Authority as necessary.
- In the ClientApp folder, make sure config.js has the correct url
- In one console window, navigate to `c:\SampleApp\ClientApp`, run the React App: `npm start` (don't forget to do a `npm install`)
- In another console window, navigate to `c:\SampleApp\`, run the web application: `dotnet run`
- Visit the local webserver at https://localhost:5001/ and sign in.


# Contributors

Initially contributed by [@andrewjboyd](https://github.com/andrewjboyd). Thanks Andrew!
