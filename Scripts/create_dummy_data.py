import os
import numpy as np
import random
import statistics

# Function to create a file with random dribble numbers
def create_dribble_number_file(filename, repetitions, index=1):
    """
    Generates random dribble numbers and writes them to a file, along with game statistics.

    Args:
        filename (str): The name of the output file.
        repetitions (int): The number of dribble numbers to generate.
        index (int, optional): The index of the current game. Defaults to 1.
    """
    if index == 1:
        _filename, _ = os.path.splitext(filename.split("/")[-1])
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


# Function to create a file with repeated phone numbers
def create_phone_number_file( filename, phone_number, repetitions, index=1):
    """
    Creates a file with a repeated phone number and associated information.

    Args:
        title (str): The title for the phone number.
        phone_number (str): The phone number to repeat.
        repetitions (int): The number of times to repeat the phone number.
        filename (str): The name of the output file.
        index (int, optional): The index of the current entry. Defaults to 1.
    """
    if index == 1:
        _filename, _ = os.path.splitext(filename.split("/")[-1])
        with open(filename, 'w') as file:
            file.write(f"{_filename.replace('_', ' ').title()}\n\n")

    with open(filename, 'a') as file:
        file.write(f"Phone Number #{index}\n")
        for idx in range(repetitions):
            if idx == repetitions - 1:
                file.write(f"{phone_number}\n\n")
            else:
                if (idx + 1) % 5 == 0:
                    file.write(f"{phone_number},\n")
                else:
                    file.write(f"{phone_number}, ")


# Function to generate shot attempts in basketball and calculate statistics
def generate_shot_attempts(filename, num_attempts, game_number=1):
    """
    Generates random shot attempts in basketball and calculates statistics.

    Args:
        filename (str): The name of the output file.
        num_attempts (int): The number of shot attempts to generate.
        game_number (int, optional): The index of the current game. Defaults to 1.
    """
    court_width = 50
    court_length = 47
    basket_x = 0
    basket_y = court_length

    shot_attempts = []
    for _ in range(num_attempts):
        if random.random() < 0.7:
            x = np.random.uniform(-court_width / 2, court_width / 2)
            y = np.random.uniform(0, 23.75)
        else:
            angle = np.random.uniform(-np.pi / 2, np.pi / 2)
            r = np.random.uniform(23.75, court_length)
            x = basket_x + r * np.cos(angle)
            y = basket_y - r * np.sin(angle)

        if random.random() < 0.02:
            x = np.random.uniform(-court_width / 2, court_width / 2)
            y = np.random.uniform(court_length / 2, court_length)

        shot_attempts.append((x, y))

    _filename, _ = os.path.splitext(filename.split("/")[-1])
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


# Function to generate football pass attempts and calculate statistics
def generate_pass_attempts(filename, num_attempts, game_number=1):
    """
    Generates random football pass attempts and calculates statistics.

    Args:
        filename (str): The name of the output file.
        num_attempts (int): The number of pass attempts to generate.
        game_number (int, optional): The index of the current game. Defaults to 1.
    """
    field_length = 100
    field_width = 53.3
    yard_line_start = random.uniform(0, 50)

    pass_attempts = []
    for _ in range(num_attempts):
        if random.random() < 0.6:
            pass_distance = np.random.uniform(0, 20)
        else:
            pass_distance = np.random.uniform(20, 60)

        lateral_offset = np.random.uniform(-field_width / 2, field_width / 2)
        pass_attempts.append((lateral_offset, pass_distance))

    _filename, _ = os.path.splitext(filename.split("/")[-1])
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


# Function to generate defensive coverages and calculate statistics
def generate_defensive_coverages(filename, num_plays, game_number=1):
    """
    Generates random defensive coverages and calculates statistics.

    Args:
        filename (str): The name of the output file.
        num_plays (int): The number of defensive plays to generate.
        game_number (int, optional): The index of the current game. Defaults to 1.
    """
    coverages = ['Cover 0', 'Cover 1', 'Cover 2', 'Cover 3', 'Cover 4', 'Cover 6']
    defensive_plays = []
    blitzes = 0

    for _ in range(num_plays):
        coverage = random.choice(coverages)
        down = random.randint(1, 4)
        yards_to_go = random.randint(1, 20)
        blitz = random.random() < 0.3
        if blitz:
            blitzes += 1
        defensive_plays.append((coverage, down, yards_to_go, blitz))

    _filename, _ = os.path.splitext(filename.split("/")[-1])
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
                file.write(",\n")

        avg_yards_to_go = round(sum([yards_to_go for _, _, yards_to_go, _ in defensive_plays]) / num_plays, 2)
        coverage_count = {coverage: sum(1 for play in defensive_plays if play[0] == coverage) for coverage in coverages}

        file.write(f"Game {game_number} Stats:\n")
        file.write(f"Total Defensive Plays: {len(defensive_plays)}\n")
        file.write(f"Average Yards to Go: {avg_yards_to_go} yd\n")
        file.write(f"Total Blitzes: {blitzes}\n")
        for coverage in coverages:
            file.write(f"Total {coverage} Plays: {coverage_count[coverage]}\n")
        file.write("\n")

def create_directory_if_not_exists(directory_path):
    """
    Creates a directory if it doesn't already exist.

    Args:
        directory_path (str): The path to the directory to create.
    """

    if not os.path.exists(directory_path):
        os.makedirs(directory_path)

# Function to run all generators
def run_generators():
    """
    Runs all the generator functions to create sample data files.
    """
    create_directory_if_not_exists("dummy_data")
    for idx in range(5):
        create_dribble_number_file("dummy_data/Dribble_Number_Test.txt", random.randint(80, 100), index=idx+1)
        generate_shot_attempts("dummy_data/Shot_Attempts_Test.txt", random.randint(20, 40), game_number=idx+1)
        generate_pass_attempts("dummy_data/Pass_Attempts_Test.txt", random.randint(20, 40), game_number=idx+1)
        generate_defensive_coverages("dummy_data/Defensive_Coverages_Test.txt", random.randint(20, 75), game_number=idx+1)
    
    phone_numbers = ["(123) 456-7890", "(234) 567-8901" , "(345) 678-9012"]
    for idx, pn in enumerate(phone_numbers):
        create_phone_number_file("dummy_data/Phone_Numbers_Test.txt", pn, 50,  index=idx+1)



# Run all generators
run_generators()
