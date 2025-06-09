import { Form } from "react-bootstrap";

interface DropDownFilterProps {
    label: string;
    value: string;
    options: string[];
    onChange: (value: string) => void;
}

export default function DropDownFilter({
    label,
    value,
    options,
    onChange,
}: DropDownFilterProps) {
    return (
        <Form.Group className="mb-3">
            <Form.Label>{label}</Form.Label>
            <Form.Select
                value={value}
                onChange={e => onChange(e.target.value)}
            >
                {options.map(opt => (
                    <option key={opt} value={opt}>{opt}</option>
                ))}
            </Form.Select>
        </Form.Group>
    );
}