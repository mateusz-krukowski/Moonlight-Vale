import xml.etree.ElementTree as ET

# Ścieżka do pliku .tmx
input_file = "village.tmx"
output_file = "unique_tiles.txt"

def extract_unique_tile_ids(tmx_path):
    tree = ET.parse(tmx_path)
    root = tree.getroot()

    unique_ids = set()

    for layer in root.findall("layer"):
        data = layer.find("data")
        if data is not None and data.text:
            tile_ids = data.text.strip().replace('\n', '').split(',')
            for tile_id in tile_ids:
                tile_id = tile_id.strip()
                if tile_id.isdigit() and int(tile_id) != 0:  # pomijamy "0", które oznacza pusty kafelek
                    unique_ids.add(int(tile_id))
    
    return sorted(unique_ids)

if __name__ == "__main__":
    ids = extract_unique_tile_ids(input_file)

    with open(output_file, "w") as f:
        for tile_id in ids:
            f.write(f"{tile_id}\n")

    print(f"Znaleziono {len(ids)} unikalnych ID. Zapisano do '{output_file}'.")
