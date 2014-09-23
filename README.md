# Golden Ticket - School Lottery Registration and Selection

Golden Ticket is a web application that provides forms for parents to register their children for school lotteries and an administrator to perform a randomized selection process based on factors such as income, gender, and location.

The project is very much a work in progress right now, with the first stable version expected in November 2014.  

# Project History

The project began as a way to conduct the Rhode Island Pre-K Lottery process for the Rhode Island Department of Education's (RIDE) Early Childhood Education program.

The project has been conducted in three phases, based on time constraints.

## Phase 1

Created a Google Form to collect electronic registrations that were saved to a Google Spreadsheet. The team created a [tool to separate the registrations by school](http://github.com/codeforamerica/golden-ticket-splitter) to help RIDE conduct metrics gathering during the registration process.

Learning: Registrations went up 27% compared to the paper process in 2013.

## Phase 2

Using the registrations collected in Phase 1, the team created a [tool to perform the lottery selection process](http://github.com/codeforamerica/golden-ticket-console) using data from the Google Spreadsheet/CSV. It not only would created selected and waitlisted CSVs, but also intermediary CSVs showing different steps in the lottery selection process (done for accountability).

Learning: The lottery selection process ran on 1000 registrations in about 2 seconds. This saved RIDE and school administrators a day or two of work performing the process manually.

## Phase 3

This project is phase 3. In place of a Google Form and a command line tool, the application will cover the process end-to-end in the form of a web app. [Here are designs for the app.](https://codeforamerica.mybalsamiq.com/projects/ri-pre-kindergartenlottery/grid)