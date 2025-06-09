import { Form } from "react-bootstrap";

interface RangeFilterProps {
    label: string;
    min: number;
    max: number;
    value: number;
    onChange: (value: number) => void;
}

export default function RangeFilter({
    label,
    min,
    max,
    value,
    onChange,
}: RangeFilterProps) {
    return (
        <Form.Group className="mb-3">
            <div className="d-flex align-items-center justify-content-between mb-1">
                <Form.Label className="mb-0">{label}</Form.Label>
                <span style={{ minWidth: 80, textAlign: "start", fontWeight:"bold" }}>
                    £ {value},00
                </span>
            </div>
            <div className="d-flex align-items-center gap-1">
                <span style={{ minWidth: 15, textAlign: "start" }}>{min}</span>
                <Form.Range
                    min={min}
                    max={max}
                    value={value}
                    onChange={e => onChange(Number(e.target.value))}
                    className="flex-grow-1 align-items-start"
                />
                <span style={{ minWidth: 80, textAlign: "center" }}>{max}</span>
            </div>
        </Form.Group>
    );
}