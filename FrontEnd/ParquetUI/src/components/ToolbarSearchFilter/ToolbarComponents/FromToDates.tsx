import { Form } from "react-bootstrap";
import DatePicker from "react-datepicker";

interface FromToDatesProps {
    label: string;
    fromDate: Date | null;
    toDate: Date | null;
    setFrom: (date: Date | null) => void;
    setTo: (date: Date | null) => void;
    fromPlaceholder?: string;
    toPlaceholder?: string;
    dateFormat?: string;
}

export default function FromToDates({
    label,
    fromDate,
    toDate,
    setFrom,
    setTo,
    fromPlaceholder = "From",
    toPlaceholder = "To",
    dateFormat = "dd-MM-yyyy",
}: FromToDatesProps) {
    return (
        <Form.Group className="mb-3">
            <Form.Label>{label}</Form.Label>
            <div className="d-flex gap-2">
                <DatePicker
                    selected={fromDate}
                    onChange={setFrom}
                    selectsStart
                    startDate={fromDate}
                    endDate={toDate}
                    placeholderText={fromPlaceholder}
                    className="form-control"
                    dateFormat={dateFormat}
                    isClearable
                />
                <span className="align-self-center">-</span>
                <DatePicker
                    selected={toDate}
                    onChange={setTo}
                    selectsEnd
                    startDate={fromDate}
                    endDate={toDate}
                    minDate={fromDate ?? undefined}
                    placeholderText={toPlaceholder}
                    className="form-control"
                    dateFormat={dateFormat}
                    isClearable
                />
            </div>
        </Form.Group>
    );
}