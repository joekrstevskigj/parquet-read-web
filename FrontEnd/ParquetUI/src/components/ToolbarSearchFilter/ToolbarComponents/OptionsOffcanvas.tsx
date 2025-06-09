import type { SearchOptionFields } from "@/types/SearchOptionFields";
import { Form, Offcanvas } from "react-bootstrap";

interface OptionsOffcanvasProps {
    show: boolean;
    onHide: () => void;
    searchOptionFields: SearchOptionFields;
    setSearchFields: React.Dispatch<React.SetStateAction<SearchOptionFields>>;
}

function toLabel(key: string) {
    return key
        .replace(/^search/, "") // Remove 'search' prefix
        .replace(/([A-Z])/g, " $1") // Add space before capital letters
        .replace(/^./, str => str.toUpperCase()) // Capitalize first letter
        .trim();
}

export default function OptionsOffcanvas({
    show,
    onHide,
    searchOptionFields,
    setSearchFields
}: OptionsOffcanvasProps) {
    
    const handleSwitchChange = (key: keyof SearchOptionFields) => (e: React.ChangeEvent<HTMLInputElement>) => {
        setSearchFields({
            ...searchOptionFields,
            [key]: e.target.checked
        });
    };

    return (
        <Offcanvas show={show} onHide={onHide} scroll={true} style={{ zIndex: 2000 }}>
            <Offcanvas.Header closeButton>
                <Offcanvas.Title>Search Options</Offcanvas.Title>
            </Offcanvas.Header>
            <Offcanvas.Body>
                {Object.keys(searchOptionFields).map(key => (
                    <Form.Switch
                        key={key}
                        id={`offcanvas-chk-${key}`}
                        label={toLabel(key)}
                        checked={searchOptionFields[key as keyof SearchOptionFields]}
                        onChange={handleSwitchChange(key as keyof SearchOptionFields)}
                        className="mb-2"
                    />
                ))}
            </Offcanvas.Body>
        </Offcanvas>
    );
}