﻿import sys
import time
import threading

# Define a mechanism for sending messages to the IDE.
# ASCII character codes 17 and 18 are used as delimiters
# around the message, so the IDE can recognise messages.
def send_message(subject, content):
  sys.stdout.write(chr(17) + str(subject) + ":" + str(content) + chr(18))
  sys.stdout.flush()

# Define a mechanism to parse messages received from the IDE
def wait_for_message():
    raw = sys.stdin.readline().rstrip("\r\n")
    index = raw.find(":")
    if index >= 0:
        subject = raw[0:index]
        content = raw[index+1:]
    else:
        subject = ""
        content = raw
    return (subject, content)

def wait_for_specific_message(specific_subject):
    while True:
        (subject, content) = wait_for_message()
        if subject == specific_subject:
            return (subject, content)
        else:
            time.sleep(0.1)

# Take control of the "input" function, so we can trigger the UI input box.
def my_input(prompt=''):
    send_message("INPUT", prompt)
    (subject, content) = wait_for_specific_message("INPUT_RESPONSE")
    return content

__builtins__.input = my_input

# Enable tracing / debugging / step-through
def trace_lines(frame, event, arg):
    if event == "line" and frame.f_code.co_filename == "<string>":
        send_message("LINE", frame.f_lineno)
        wait_for_specific_message("CONTINUE")

def trace_calls(frame, event, arg):
    if event == "call" and frame.f_code.co_filename == "<string>":
        return trace_lines

sys.settrace(trace_calls)

# Continue executing the program written in the IDE, which will have been written
# to disk and passed in to startup.py as the first agument.
exec(open(sys.argv[1]).read())
