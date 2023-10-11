/*
 *  Copyright 2018 Ulf Adams.
 *  
 *  Modifications for ECMAScript / RFC 8785 by Anders Rundgren
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 *
 */
package org.webpki.jcs;

import java.io.IOException;

/**
 * JSON as specified by ECMAScript
 */
public final class NumberToJSON {
    /**
     * Formats a number according to ECMAScript.
     * <p>
     * This code is emulating 7.1.12.1 of the EcmaScript V6 specification.
     * </p>
     * 
     * @param value Value to be formatted
     * @return String representation
     */
    public static String serializeNumber(double value) throws IOException {
        // First, handle the JSON cases.
        if (value == 0.0) {
            return "0";
        }
        if (Double.isNaN(value) || Double.isInfinite(value)) {
            throw new IOException("NaN/Infinity not allowed in JSON");
        }
        return DoubleCoreSerializer.serialize(value, true);
    }
}
