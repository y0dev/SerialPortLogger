import os

def create_phone_number_file(title, phone_number, repetitions, filename, index=1):
    """
    Creates a text file with the specified phone number repeated the given number of times.

    Args:
        phone_number (str): The phone number to repeat.
        repetitions (int): The number of times to repeat the phone number.
        filename (str): The name of the output file.
    """
    if index == 1:
        _filename, _ = os.path.splitext(filename)
        with open(filename, 'w') as file:
            file.write(f"{_filename.replace('_', ' ').title()}\n\n")

    with open(filename, 'a') as file:
        file.write(f"{title} #{index}\n")
        for idx in range(repetitions):
            if idx == repetitions - 1:
                file.write(f"{phone_number}\n\n")
            else:
                if (idx + 1) % 5 == 0:
                    file.write(f"{phone_number},\n")
                else:
                    file.write(f"{phone_number}, ")


phone_numbers = ["(123) 456-7890", "(234) 567-8901" , "(345) 678-9012"]

repetitions = 75
filename = "phone_numbers.txt"
title = "Phone Number"
for idx, phone_number in enumerate(phone_numbers):
    create_phone_number_file(title, phone_number, repetitions, filename, index=idx+1)