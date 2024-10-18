import os

import numpy as np
import random
import statistics

def create_dribble_number_file(filename, repetitions, index=1):
    """
    Creates a text file with random numbers between 8 and 12, representing the number of dribbles before taking a shot.

    Args:
    filename (str): The name of the output file.
    repetitions (int): The number of random numbers to generate.
    """
    if index == 1:
        _filename, _ = os.path.splitext(filename)
        with open(filename, 'w') as file:
            file.write(f"{_filename.replace('_', ' ').title()}\n\n")

    dribbles_per_pos = []
    with open(filename, 'a') as file:
        file.write(f"Game #{index}\n")
        for idx in range(repetitions):
            dribble_number = random.randint(1, 15)
            dribbles_per_pos.append(dribble_number)
            if idx == repetitions - 1:
                file.write(f"{dribble_number}\n\n")
            else:
                if (idx + 1) % 30 == 0:
                    file.write(f"{dribble_number},\n")
                else:
                    file.write(f"{dribble_number}, ")
    
    
    with open(filename, 'a') as file:
        file.write(f"Game {index} Stats:\n")
        file.write(f"Total Possessions: {len(dribbles_per_pos)}\n")
        file.write(f"Dribble Average Per Possession: {round(statistics.mean(dribbles_per_pos), 2)}\n")
        file.write(f"Most Dribbles in a Single Possession: {max(dribbles_per_pos)}\n")
        file.write(f"Least Dribbles in a Single Possession: {min(dribbles_per_pos)}\n")
        file.write(f"Number of times the Max Dribbles Occurred: {dribbles_per_pos.count(max(dribbles_per_pos))}\n")
        file.write(f"Number of times the Least Dribbles Occurred: {dribbles_per_pos.count(min(dribbles_per_pos))}\n\n")



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


# phone_numbers = ["(123) 456-7890", "(234) 567-8901" , "(345) 678-9012"]

# repetitions = 75
# filename = "phone_numbers.txt"
# title = "Phone Number"
# for idx, phone_number in enumerate(phone_numbers):
#     create_phone_number_file(title, phone_number, repetitions, filename, index=idx+1)


# Example usage:
filename = "dribble_numbers.txt"
repetitions = 75

# for idx in range(5):
#     create_dribble_number_file(filename, random.randint(80, 100), index=idx+1)


def generate_shot_attempts(filename, num_attempts, game_number=1):
    # Half-court dimensions (in feet)
    court_width = 50  # Court width (NBA regulation is 50 ft)
    court_length = 47  # Half-court length (NBA regulation is 47 ft)

    # Basket position
    basket_x = 0  # Hoop at the center of the court width
    basket_y = court_length  # Hoop at the end of the half court

    shot_attempts = []

    # Generate shot attempts
    for _ in range(num_attempts):
        if random.random() < 0.7:  # 70% chance of being a closer shot
            x = np.random.uniform(-court_width / 2, court_width / 2)
            y = np.random.uniform(0, 23.75)  # inside 3-point line
        else:
            angle = np.random.uniform(-np.pi / 2, np.pi / 2)
            r = np.random.uniform(23.75, court_length)  # outside or near the arc
            x = basket_x + r * np.cos(angle)
            y = basket_y - r * np.sin(angle)

        # Adding rare half-court shots
        if random.random() < 0.02:
            x = np.random.uniform(-court_width / 2, court_width / 2)
            y = np.random.uniform(court_length / 2, court_length)

        shot_attempts.append((x, y))

    # Writing the shot attempts to a file
    _filename, _ = os.path.splitext(filename)
    
    if game_number == 1:
        with open(filename, 'w') as file:
            file.write(f"{_filename.replace('_', ' ').title()}\n\n")
    
    with open(filename, 'a') as file:
        file.write(f"Game #{game_number}\n")
        for idx, (x, y) in enumerate(shot_attempts):
            file.write(f"({x:.2f}, {y:.2f})")
            if idx == len(shot_attempts) - 1:
                file.write("\n\n")
            else:
                if (idx + 1) % 30 == 0:
                    file.write(",\n")
                else:
                    file.write(", ")

        # Calculate distances from the basket
        distances = [np.sqrt((x - basket_x)**2 + (y - basket_y)**2) for x, y in shot_attempts]
        
        avg_distance = round(np.mean(distances), 2)
        max_distance = round(max(distances), 2)
        min_distance = round(min(distances), 2)
        
        # Writing stats for the game
        file.write(f"Game {game_number} Stats:\n")
        file.write(f"Total Attempts: {len(shot_attempts)}\n")
        file.write(f"Average Distance from Basket: {avg_distance} ft\n")
        file.write(f"Max Distance from Basket: {max_distance} ft\n")
        file.write(f"Min Distance from Basket: {min_distance} ft\n\n")

# Example usage:
game_number = 1
num_attempts = 50
filename = "shot_attempts.txt"


for idx in range(5):
    generate_shot_attempts(filename, random.randint(20, 40), idx+1)


def generate_pass_attempts(filename, num_attempts, game_number=1):
    # Football field dimensions
    field_length = 100  # Full field length in yards
    field_width = 53.3  # Field width in yards (standard NFL width)

    # Line of scrimmage (starting point for passes)
    yard_line_start = random.uniform(0, 50)  # Between 0 and 50 yard line

    pass_attempts = []

    # Generate pass attempts
    for _ in range(num_attempts):
        # Pass can be short, medium, or long
        if random.random() < 0.6:  # 60% chance for short to medium range pass
            pass_distance = np.random.uniform(0, 20)  # short to mid-range pass (0-20 yards)
        else:
            pass_distance = np.random.uniform(20, 60)  # long-range pass (20-60 yards)

        # Pass direction (horizontal spread on the field)
        lateral_offset = np.random.uniform(-field_width / 2, field_width / 2)  # spread across the width

        pass_attempts.append((lateral_offset, pass_distance))

    # Writing the pass attempts to a file
    _filename, _ = os.path.splitext(filename)
    
    if game_number == 1:
        with open(filename, 'w') as file:
            file.write(f"{_filename.replace('_', ' ').title()}\n\n")
    
    with open(filename, 'a') as file:
        file.write(f"Game #{game_number}\n")
        for idx, (offset, distance) in enumerate(pass_attempts):
            file.write(f"(Offset: {offset:.2f} yd, Distance: {distance:.2f} yd)")
            if idx == len(pass_attempts) - 1:
                file.write("\n\n")
            else:
                if (idx + 1) % 30 == 0:
                    file.write(",\n")
                else:
                    file.write(", ")

        # Calculate average, max, and min pass distances
        distances = [distance for _, distance in pass_attempts]
        
        avg_distance = round(np.mean(distances), 2)
        max_distance = round(max(distances), 2)
        min_distance = round(min(distances), 2)
        
        # Writing stats for the game
        file.write(f"Game {game_number} Stats:\n")
        file.write(f"Total Pass Attempts: {len(pass_attempts)}\n")
        file.write(f"Average Yard Per Pass: {avg_distance} yd\n")
        file.write(f"Longest Pass: {max_distance} yd\n")
        file.write(f"Shortest Pass: {min_distance} yd\n\n")


filename = "pass_attempts.txt"

for idx in range(5):
    generate_pass_attempts(filename, random.randint(20, 40), idx+1)

def generate_defensive_coverages(filename, num_plays, game_number=1):
    # Possible defensive coverages
    coverages = ['Cover 0', 'Cover 1', 'Cover 2', 'Cover 3', 'Cover 4', 'Cover 6']
    
    defensive_plays = []
    blitzes = 0

    # Generate defensive coverages for each play
    for _ in range(num_plays):
        # Randomly choose coverage type
        coverage = random.choice(coverages)
        
        # Random down and distance
        down = random.randint(1, 4)  # 1st to 4th down
        yards_to_go = random.randint(1, 20)  # Yards to go (1 to 20)
        
        # Random blitz occurrence (approx 30% chance of a blitz)
        blitz = random.random() < 0.3
        if blitz:
            blitzes += 1
        
        defensive_plays.append((coverage, down, yards_to_go, blitz))

    # Writing the defensive plays to a file
    _filename, _ = os.path.splitext(filename)
    
    if game_number == 1:
        with open(filename, 'w') as file:
            file.write(f"{_filename.replace('_', ' ').title()}\n\n")
    
    with open(filename, 'a') as file:
        file.write(f"Game #{game_number}\n")
        for idx, (coverage, down, yards_to_go, blitz) in enumerate(defensive_plays):
            blitz_text = "Yes" if blitz else "No"
            file.write(f"Down: {down}, Distance: {yards_to_go} yd, Coverage: {coverage}, Blitz: {blitz_text}")
            if idx == len(defensive_plays) - 1:
                file.write("\n\n")
            else:
                if (idx + 1) % 30 == 0:
                    file.write(",\n")
                else:
                    file.write(", ")

        # Calculate average down and distance
        avg_yards_to_go = round(sum([yards_to_go for _, _, yards_to_go, _ in defensive_plays]) / num_plays, 2)

        # Count of coverages used
        coverage_count = {coverage: sum(1 for play in defensive_plays if play[0] == coverage) for coverage in coverages}
        
        # Writing stats for the game
        file.write(f"Game {game_number} Stats:\n")
        file.write(f"Total Defensive Plays: {len(defensive_plays)}\n")
        file.write(f"Average Yards to Go: {avg_yards_to_go} yd\n")
        file.write(f"Total Blitzes: {blitzes}\n")
        file.write("Coverage Breakdown:\n")
        for coverage, count in coverage_count.items():
            file.write(f"  {coverage}: {count} times\n")
        file.write("\n")


# Example usage:
filename = "defensive_coverages.txt"
for idx in range(5):
    generate_defensive_coverages(filename, random.randint(20, 40), idx+1)
