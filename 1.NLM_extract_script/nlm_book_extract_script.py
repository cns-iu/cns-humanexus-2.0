import fitz  # PyMuPDF
import os

def extract_images_from_pdf(pdf_path, output_folder):
    os.makedirs(output_folder, exist_ok=True)
    doc = fitz.open(pdf_path)
    image_count = 0

    for page_index in range(len(doc)):
        page = doc[page_index]
        images = page.get_images(full=True)

        for img_index, img in enumerate(images):
            xref = img[0]
            base_image = doc.extract_image(xref)
            image_bytes = base_image["image"]
            image_ext = base_image["ext"]
            image_filename = f"page{page_index+1}_img{img_index+1}.{image_ext}"

            with open(os.path.join(output_folder, image_filename), "wb") as f:
                f.write(image_bytes)

            image_count += 1

    print(f"Extracted {image_count} images to '{output_folder}'.")

# Example usage
extract_images_from_pdf(r"C:\Users\parth\Downloads\HIDDENTREASURE_NLM_BlastBooks.pdf", r"C:\Users\parth\Downloads\HUmanexus\NLM Extracts")
