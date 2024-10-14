import json
import os
import xml.etree.ElementTree as ET

def json_to_xml(json_obj, root_tag='root'):
    def build_xml_element(element, data):
        if isinstance(data, dict):
            for key, value in data.items():
                if key == '@attributes' and isinstance(value, dict):
                    # Set attributes on the current element
                    element.attrib.update(value)
                else:
                    # Handle dictionary entries (possibly nested)
                    if isinstance(value, dict):
                        sub_element = ET.SubElement(element, key)
                        build_xml_element(sub_element, value)
                    elif isinstance(value, list):
                        for item in value:
                            sub_element = ET.SubElement(element, key)
                            build_xml_element(sub_element, item)
                    else:
                        sub_element = ET.SubElement(element, key)
                        sub_element.text = str(value)
        elif isinstance(data, list):
            for item in data:
                singular_tag = element.tag[:-1] if element.tag.endswith('s') else element.tag
                sub_element = ET.SubElement(element, singular_tag)
                build_xml_element(sub_element, item)
        else:
            element.text = str(data)

    root = ET.Element(root_tag)
    build_xml_element(root, json_obj)
    return ET.ElementTree(root)

def xml_to_json(xml_data):
    def parse_element(element):
        parsed_data = {}
        # Capture attributes of the current element
        if element.attrib:
            parsed_data["@attributes"] = {k: v for k, v in element.attrib.items()}
        if list(element):  # If the element has children
            for sub_element in element:
                sub_element_data = parse_element(sub_element)
                if sub_element.tag not in parsed_data:
                    parsed_data[sub_element.tag] = sub_element_data
                else:
                    if not isinstance(parsed_data[sub_element.tag], list):
                        parsed_data[sub_element.tag] = [parsed_data[sub_element.tag]]
                    parsed_data[sub_element.tag].append(sub_element_data)
        else:  # If the element is a leaf node
            parsed_data = element.text.strip() if element.text else None
        return parsed_data

    root = ET.fromstring(xml_data)
    json_data = {root.tag: parse_element(root)}
    
    return json_data

def pretty_print_xml(elem, level=0):
    indent = "  " * level
    if len(elem):
        if not elem.text or not elem.text.strip():
            elem.text = "\n" + indent + "  "
        if not elem.tail or not elem.tail.strip():
            elem.tail = "\n" + indent
        for child in elem:
            pretty_print_xml(child, level + 1)
        if not child.tail or not child.tail.strip():
            child.tail = "\n" + indent
    else:
        if level and (not elem.tail or not elem.tail.strip()):
            elem.tail = "\n" + indent

def convert_file(input_file):
    file_name, file_extension = os.path.splitext(input_file)

    if file_extension.lower() == '.json':
        with open(input_file, 'r') as json_file:
            json_data = json.load(json_file)
            xml_tree = json_to_xml(json_data, 'Configuration')
            pretty_print_xml(xml_tree.getroot())
            output_file = file_name + '.xml'
            xml_tree.write(output_file, encoding='utf-8', xml_declaration=True)
            print(f"Converted JSON to XML: {output_file}")

    elif file_extension.lower() == '.xml':
        with open(input_file, 'r') as xml_file:
            xml_data = xml_file.read()
            json_data = xml_to_json(xml_data)
            output_file = file_name + '.json'
            with open(output_file, 'w') as json_file:
                json.dump(json_data, json_file, indent=4)
            print(f"Converted XML to JSON: {output_file}")

    else:
        print("Unsupported file format. Please provide a .json or .xml file.")

if __name__ == "__main__":
    input_file = input("Enter the file name (with extension) for conversion: ")
    convert_file(input_file)
