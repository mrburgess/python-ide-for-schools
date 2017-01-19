import sys

# Define a mechanism for sending messages to the IDE.
# ASCII character codes 17 and 18 are used as delimiters
# around the message, so the IDE can recognise messages.
def send_message(message):
  sys.stdout.write(chr(17) + message + chr(18))
  sys.stdout.flush()

# Take control of the "input" function, so we can trigger the UI input box.
def my_input(prompt=''):
  send_message("INPUT:" + prompt)
  return sys.stdin.readline().rstrip('\r\n')

__builtins__.input = my_input

# Enable tracing / debugging
def trace_lines(frame, event, arg):
	if event == "line" and frame.f_code.co_filename == "<string>":
		send_message("LINE:" + str(frame.f_lineno))

def trace_calls(frame, event, arg):
	if event == "call" and frame.f_code.co_filename == "<string>":
		return trace_lines

sys.settrace(trace_calls)

# Continue executing the program written in the IDE, which will have been written
# to disk and passed in to startup.py as the first agument.
exec(open(sys.argv[1]).read())