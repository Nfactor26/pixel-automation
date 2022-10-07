# Pixel.Automation
Pixel Automation is a open source tool written in C# .net core for creating automation test cases without writing code. It provides a  design time environment where you configure the actions to be done on UI elements in a simple and efficient manner e.g. clicking a button or enter some value in a textbox. It integrates with popular automation frameworks like Selenium, Playwright, Microsoft UI Automation, OpenCV, etc. to provide the automation capabilities and can be extended to integrate with other frameworks easily via a plugin model. It also provides C# based scripting environment to augment the configuration when required.


# Features
 - Create automation test cases without writing code. 
 - Automate both web and desktop applications and even combine different applications together in a single test case.
 - Capture control details by pointing and cliking on UI elements and then add interactions to them e.g. click , type, etc all using drag drop and configuration.
 - Use Image based automation for custom controls that can't be looked up using api.
 - Simulate Mouse and Keyboard to interact with controls when controls can be looked up using api but api based approach don't work.
 - Reuse a set of automated steps in multiple test cases to avoid repetation using "Prefabs".
 - Configurable Retry attempts for control lookup.
 - No need to recompile and start over when things go wrong. Individual steps can be executed on demand as you change configuration.
 - Visual debugging of control lookup to understand issues with lookup configuration.
 - Integrated reports for test case executions.
 - Console based client to run automation test cases
 
# Why should you use Pixel Automation ?
Writing test cases is not an easy task and maintaing them is even difficult. Pixel Automation enables you to create automation test cases much faster then writing code and at the same time tries to handle the automation framework specific complexities so that users can focus on writing test cases. It builds upon automation frameworks that you have already been using e.g. Selenium, Playwright, Microsoft UI Automation, OpenCV for image based automation, etc. It doesn't tries to hide automation framework specific api's in to another abstraction but exposes these api's as configuration instead. As a result, if you are already familiar with these frameworks, you don't have to rediscover things. New plugins can be easily created to integrated with other automation frameworks. Additionally, it comes inbuilt with a reporting system to understand how your test cases are performing.

# Current status of project
Project is over 95% feature complete. I am looking for people to try and provide feedback. Please reach out if intrested. 
