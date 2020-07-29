# BlazorFormManager

BlazorFormManager is an open-source Razor Class Library (RCL) for (both client and
server-side) Blazor projects. It provides core functionalities for handling AJAX form
submissions with zero or more files, and report back data upload progress. It does so
by enhancing the existing functionalities of an EditForm, including client-side
validations, form data and file upload progress report, abortion of an on-going
upload, and console logging support for troubleshooting.

It is flexible enough to allow advanced control, such as setting HTTP request headers,
over instances of the XMLHttpRequest object used to send requests, all from the C#/.NET
perspective.

## Quick start

Clone this repository into a directory on your device:

`> git clone https://github.com/bigabdoul/BlazorFormManager.git`

There are 3 projects found under the directory BlazorFormManager and the structure is
similar to:

- BlazorFormManager
  - src
    - BlazorFormManager
    - Demos
      - BlazorFormManager.Demo.Client
      - BlazorFormManager.Demo.Server

In the project's root directory you can find a Visual Studio Solution (.sln) file.
To launch with Visual Studio, double-click that file and make sure
`BlazorFormManager.Demo.Server` is the start-up project. Press CTRL+F5 (or the
appropriate key combination on your device).

## Demonstration projects

The demonstration (or demo) projects have been created with Visual Studio 2019's Blazor
WebAssembly App template (ASP.NET Core 3.1 hosted with authentication using individual
user accounts).

## Walkthrough documentation

Check out the [Walkthrough](WALK-THROUGH.md) file for detailed instructions that guide you through
building a set of projects similar to the demo projects found in this repository.

## Engage, contribute, and give feedback

If you want to make improvements to this project, some of the best ways to contribute are
to try things out, file issues, join in design conversations, and make pull-requests.
